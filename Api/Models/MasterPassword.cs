using System.ComponentModel.DataAnnotations;

namespace CyberPass.Models
{
    public class MasterPassword
    {
        [Key]
        public int Id { get; set; }

        // The hashed and encrypted master password (random key)
        [Required]
        public byte[] EncryptedRandomKey { get; set; }

        // Salt used for hashing the password
        [Required]
        public byte[] Salt { get; set; }

        // Nonce used for encryption
        [Required]
        public byte[] RandomNonce { get; set; }

        // Nonce used for the key encryption
        [Required]
        public byte[] KeyNonce { get; set; }
    }
}
