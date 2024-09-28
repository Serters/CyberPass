using CyberPass.Data;
using CyberPass.DTOs;
using CyberPass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CyberPass.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PasswordEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/passwordentry
        [Authorize]
        [HttpPost]
        public IActionResult CreatePasswordEntry([FromBody] PasswordEntryDTO request)
        {
            if (request == null)
            {
                return BadRequest("Password entry cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Website))
            {
                return BadRequest("Username, Password, and Website are required.");
            }

            // Retrieve the master password entry (assuming only one exists)
            var masterPasswordEntry = _context.MasterPassword.FirstOrDefault();
            if (masterPasswordEntry == null)
            {
                return BadRequest("Master password not set. Please create a master password first.");
            }

            // Encrypt the password before storing
            byte[] encryptedPassword = EncryptPassword(request.Password, masterPasswordEntry.EncryptedRandomKey);

            // Create the PasswordEntry object
            var entry = new PasswordEntry
            {
                Username = request.Username,
                EncryptedPassword = encryptedPassword,
                Website = request.Website,
                Note = request.Note,
                FolderId = request.FolderId, // Optional
                Date = DateTime.UtcNow // Set the creation date
            };

            // Add the new password entry to the database
            _context.PasswordEntries.Add(entry);
            _context.SaveChanges();

            return CreatedAtAction(nameof(CreatePasswordEntry), new { id = entry.Id }, "Password entry created successfully.");
        }


        [Authorize]
        [HttpGet]
        public IActionResult GetAllPasswordEntries()
        {
            // Retrieve the master password entry (assuming only one exists)
            var masterPasswordEntry = _context.MasterPassword.FirstOrDefault();
            if (masterPasswordEntry == null)
            {
                return BadRequest("Master password not set. Please create a master password first.");
            }

            // Retrieve all password entries
            var entries = _context.PasswordEntries.ToList();
            var decryptedEntries = entries.Select(entry => new
            {
                entry.Id,
                entry.Username,
                DecryptedPassword = DecryptPassword(entry.EncryptedPassword, masterPasswordEntry.EncryptedRandomKey),
                entry.Website,
                entry.Note,
                entry.FolderId,
                entry.Date // Include the Date property
            }).ToList();

            return Ok(decryptedEntries);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePasswordEntry(int id)
        {
            var entry = await _context.PasswordEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound("Password entry not found.");
            }

            // Create a new OldPasswords instance
            var oldPassword = new OldPassword
            {
                EncryptedPassword = entry.EncryptedPassword
            };

            // Add the old password to the OldPasswords table
            _context.OldPasswords.Add(oldPassword);

            // Remove the PasswordEntry
            _context.PasswordEntries.Remove(entry);
            await _context.SaveChangesAsync(); // Save all changes

            return NoContent(); // Return 204 No Content
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePasswordEntry(int id, [FromBody] PasswordEntryDTO request)
        {
            if (request == null)
            {
                return BadRequest("Password entry cannot be null.");
            }

            var entry = await _context.PasswordEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound("Password entry not found.");
            }

            // Save the current encrypted password to OldPasswords if the password is changing
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var oldPassword = new OldPassword
                {
                    EncryptedPassword = entry.EncryptedPassword
                };

                // Add the old password to the OldPasswords table
                _context.OldPasswords.Add(oldPassword);
            }

            // Update properties
            entry.Username = request.Username;
            entry.Website = request.Website;
            entry.Note = request.Note;
            entry.FolderId = request.FolderId;

            // Retrieve the master password entry (assuming only one exists)
            var masterPasswordEntry = await _context.MasterPassword.FirstOrDefaultAsync();
            if (masterPasswordEntry == null)
            {
                return BadRequest("Master password not set. Please create a master password first.");
            }

            // Encrypt the new password if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                entry.EncryptedPassword = EncryptPassword(request.Password, masterPasswordEntry.EncryptedRandomKey);
            }

            // Update the Date to the current date and time
            entry.Date = DateTime.UtcNow;

            await _context.SaveChangesAsync(); // Save all changes

            return NoContent(); // Return 204 No Content
        }

        // GET api/passwordentry/folder/{folderId}
        [Authorize]
        [HttpGet("folder/{folderId}")]
        public IActionResult GetPasswordEntriesByFolderId(int folderId)
        {
            // Retrieve the master password entry (assuming only one exists)
            var masterPasswordEntry = _context.MasterPassword.FirstOrDefault();
            if (masterPasswordEntry == null)
            {
                return BadRequest("Master password not set. Please create a master password first.");
            }

            // Retrieve password entries for the specified folder
            var entries = _context.PasswordEntries
                .Where(entry => entry.FolderId == folderId)
                .ToList();

            var decryptedEntries = entries.Select(entry => new
            {
                entry.Id,
                entry.Username,
                DecryptedPassword = DecryptPassword(entry.EncryptedPassword, masterPasswordEntry.EncryptedRandomKey),
                entry.Website,
                entry.Note,
                entry.FolderId,
                entry.Date // Include the Date property
            }).ToList();

            return Ok(decryptedEntries);
        }


        //------

        // Method to encrypt the password using the master key
        private byte[] EncryptPassword(string password, byte[] masterKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = masterKey; // Use the master key
                aes.GenerateIV(); // Generate a new IV for encryption

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length); // Prepend the IV to the encrypted data

                        using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var writer = new StreamWriter(cryptoStream))
                            {
                                writer.Write(password);
                            }
                        }

                        return ms.ToArray(); // Return the encrypted data
                    }
                }
            }
        }

        // Method to decrypt the password using the master key
        private string DecryptPassword(byte[] encryptedData, byte[] masterKey)
        {
            using (Aes aes = Aes.Create())
            {
                using (var ms = new MemoryStream(encryptedData))
                {
                    byte[] iv = new byte[16]; // Adjust based on your IV size
                    ms.Read(iv, 0, iv.Length); // Read the IV from the beginning of the stream

                    aes.Key = masterKey; // Use the master key
                    aes.IV = iv; // Set the IV

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cryptoStream))
                            {
                                return reader.ReadToEnd(); // Return the decrypted password
                            }
                        }
                    }
                }
            }
        }
    }
}