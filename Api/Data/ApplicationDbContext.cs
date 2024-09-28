using CyberPass.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPass.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MasterPassword> MasterPassword { get; set; }
        public DbSet<PasswordEntry> PasswordEntries { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<OldPassword> OldPasswords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PasswordEntry>()
                .HasOne(pe => pe.Folder)
                .WithMany(f => f.PasswordEntries)
                .HasForeignKey(pe => pe.FolderId);
        }

    }

}

