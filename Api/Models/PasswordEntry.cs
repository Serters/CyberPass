using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberPass.Models
{
    public class PasswordEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Adjust length as needed
        public string Username { get; set; }

        [Required]
        public byte[] EncryptedPassword { get; set; } // Store encrypted password

        [Required]
        [MaxLength(200)] // Adjust length as needed
        public string Website { get; set; }

        [MaxLength(500)] // Adjust length as needed
        public string Note { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        // This will act as a foreign key to the Folder model when created
        public int? FolderId { get; set; }

        [ForeignKey("FolderId")]
        public Folder Folder { get; set; }
    }
}
