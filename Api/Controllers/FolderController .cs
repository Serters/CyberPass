using CyberPass.Data;
using CyberPass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberPass.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FolderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/folder
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Folder>>> GetFolders()
        {
            return await _context.Folders.ToListAsync();
        }

        // GET: api/folder/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Folder>> GetFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);

            if (folder == null)
            {
                return NotFound();
            }

            return folder;
        }

        // POST: api/folder
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Folder>> CreateFolder(Folder folder)
        {
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, folder);
        }

        // PUT: api/folder/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(int id, Folder folder)
        {
            if (id != folder.Id)
            {
                return BadRequest();
            }

            _context.Entry(folder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FolderExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/folder/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return NotFound("Folder not found.");
            }

            // Retrieve all password entries associated with this folder
            var passwordEntries = _context.PasswordEntries.Where(pe => pe.FolderId == id).ToList();

            // Set FolderId to null for all related password entries
            foreach (var entry in passwordEntries)
            {
                entry.FolderId = null;
            }

            // Save changes for updating password entries
            await _context.SaveChangesAsync();

            // Remove the folder
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync(); // Save all changes

            return NoContent(); // Return 204 No Content
        }


        private bool FolderExists(int id)
        {
            return _context.Folders.Any(e => e.Id == id);
        }
    }
}
