namespace CyberPass.Models
{
    public class OldPassword
    {
        public int Id { get; set; }                 // Unique identifier for the old password entry
        public byte[] EncryptedPassword { get; set; } // Store the encrypted old password
    }
}
