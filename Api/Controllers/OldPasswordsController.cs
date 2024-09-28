using CyberPass.Data;
using CyberPass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CyberPass.Utilities;

namespace CyberPass.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OldPasswordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OldPasswordsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/oldpasswords
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetOldPasswords()
        {
            // Retrieve the master password entry (assuming only one exists)
            var masterPasswordEntry = await _context.MasterPassword.FirstOrDefaultAsync();
            if (masterPasswordEntry == null)
            {
                return BadRequest("Master password not set. Please create a master password first.");
            }

            // Retrieve old passwords
            var oldPasswords = await _context.OldPasswords.ToListAsync();

            // Decrypt each old password
            var decryptedPasswords = new List<string>();
            foreach (var oldPassword in oldPasswords)
            {
                var decryptedPassword = Utilities.Encryption.DecryptPassword(oldPassword.EncryptedPassword, masterPasswordEntry.EncryptedRandomKey);
                decryptedPasswords.Add(decryptedPassword);
            }

            return Ok(decryptedPasswords); // Return the decrypted passwords
        }

    }
}
