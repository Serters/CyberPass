using CyberPass.Data;
using CyberPass.DTOs;
using CyberPass.Models;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using CyberPass.Utilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace CyberPass.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterPasswordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        public MasterPasswordController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // POST api/masterpassword
        [HttpPost]
        public IActionResult CreateMasterPassword([FromBody] string masterPassword)
        {
            if (string.IsNullOrWhiteSpace(masterPassword))
            {
                return BadRequest("Master password cannot be empty.");
            }

            // Check if a master password already exists
            if (_context.MasterPassword.Any())
            {
                return Conflict("A master password already exists.");
            }

            // Create and store the new master password
            byte[] salt = GenerateSalt();
            byte[] hashedPassword = HashPassword(masterPassword, salt);

            var newPassword = new MasterPassword
            {
                EncryptedRandomKey = hashedPassword, // Store the hashed password
                Salt = salt,
                RandomNonce = GenerateNonce(), // Replace with your nonce generation method
                KeyNonce = GenerateNonce()     // Replace with your nonce generation method
            };

            _context.MasterPassword.Add(newPassword);
            _context.SaveChanges();

            return CreatedAtAction(nameof(CreateMasterPassword), new { id = newPassword.Id }, "Master password created successfully.");
        }

        // PUT api/masterpassword
        [Authorize]
        [HttpPut]
        public IActionResult UpdateMasterPassword([FromBody] MasterPasswordDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CurrentMasterPassword) || string.IsNullOrWhiteSpace(request.NewMasterPassword))
            {
                return BadRequest("Current and new master passwords must be provided.");
            }

            // Retrieve the existing master password entry
            var existingEntry = _context.MasterPassword.FirstOrDefault();
            if (existingEntry == null)
            {
                return NotFound("Master password not found. Please create one first.");
            }

            // Verify the current master password
            byte[] hashedCurrentPassword = HashPassword(request.CurrentMasterPassword, existingEntry.Salt);
            if (!hashedCurrentPassword.SequenceEqual(existingEntry.EncryptedRandomKey))
            {
                return Unauthorized("Current master password is incorrect.");
            }

            // Retrieve all password entries for decryption
            var passwordEntries = _context.PasswordEntries.ToList();
            var oldPasswords = _context.OldPasswords.ToList(); // Retrieve old passwords

            // Hash and store the new master password
            byte[] newSalt = GenerateSalt();
            byte[] hashedNewPassword = HashPassword(request.NewMasterPassword, newSalt);

            // Decrypt and re-encrypt all current password entries
            foreach (var entry in passwordEntries)
            {
                string decryptedPassword = Utilities.Encryption.DecryptPassword(entry.EncryptedPassword, existingEntry.EncryptedRandomKey);
                byte[] newEncryptedPassword = Utilities.Encryption.EncryptPassword(decryptedPassword, hashedNewPassword);
                entry.EncryptedPassword = newEncryptedPassword;
            }

            // Re-encrypt all old passwords
            foreach (var oldPassword in oldPasswords)
            {
                byte[] newEncryptedOldPassword = Utilities.Encryption.EncryptPassword(Utilities.Encryption.DecryptPassword(oldPassword.EncryptedPassword, existingEntry.EncryptedRandomKey), hashedNewPassword);
                oldPassword.EncryptedPassword = newEncryptedOldPassword;
            }

            existingEntry.EncryptedRandomKey = hashedNewPassword; // Update with new hashed password
            existingEntry.Salt = newSalt; // Update salt
            existingEntry.RandomNonce = GenerateNonce(); // Optionally update nonce
            existingEntry.KeyNonce = GenerateNonce(); // Optionally update nonce

            _context.SaveChanges();

            return NoContent(); // Return 204 No Content
        }

        //------------------------------------

        [HttpPost("login")]
        public IActionResult Login([FromBody] string masterPassword)
        {
            // Retrieve the existing master password entry
            var existingEntry = _context.MasterPassword.FirstOrDefault();
            if (existingEntry == null || !VerifyPassword(masterPassword, existingEntry))
            {
                return Unauthorized("Invalid master password.");
            }

            var token = GenerateJwtToken(existingEntry); // Method to generate token
            return Ok(new { Token = token });
        }

        private bool VerifyPassword(string password, MasterPassword entry)
        {
            byte[] hashedPassword = HashPassword(password, entry.Salt);
            return hashedPassword.SequenceEqual(entry.EncryptedRandomKey);
        }

        private string GenerateJwtToken(MasterPassword entry)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, entry.Id.ToString()),
        new Claim(ClaimTypes.Name, entry.EncryptedRandomKey.ToString())
    };

            // Retrieve the secret key from configuration
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("JwtSettings:Issuer"),
                audience: _configuration.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(3000),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        //----------------------------------
        // Generate a cryptographic salt
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16]; // 128-bit salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Hash the password using Argon2id
        private byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,  // Multithreading options
                MemorySize = 1024 * 1024, // 1 GB of memory usage
                Iterations = 4            // Number of iterations
            };

            return argon2.GetBytes(32); // 256-bit key
        }

        // Method for generating nonce
        private byte[] GenerateNonce()
        {
            byte[] nonce = new byte[12]; // Nonce size for AES-GCM
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }
            return nonce;
        }
    }

}

