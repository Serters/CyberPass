namespace CyberPass.DTOs
{
    public class PasswordEntryDTO
    {
        public string Username { get; set; }
        public string Password { get; set; } // Plain text password to be encrypted
        public string Website { get; set; }
        public string Note { get; set; }
        public int? FolderId { get; set; } // Optional
        public DateTime Date { get; set; } // Date of the password entry
    }
}
