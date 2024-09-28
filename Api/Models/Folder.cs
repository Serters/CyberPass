namespace CyberPass.Models
{
    public class Folder
    {
        public int Id { get; set; }     // Unique identifier for the folder
        public string Name { get; set; } // Name of the folder

        public ICollection<PasswordEntry> PasswordEntries { get; set; } = new List<PasswordEntry>();
    }

}
