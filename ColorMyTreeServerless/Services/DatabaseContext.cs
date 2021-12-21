using ColorMyTree.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace ColorMyTree.Services
{
    public class DatabaseContext : DbContext
    {
        public DbSet<CmtUser> Users { get; set; }
        public DbSet<Gift> Gifts { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CmtUser>()
                .HasPartitionKey(u => u.Id)
                .HasNoDiscriminator()
                .ToContainer("Users");

            modelBuilder.Entity<CmtUser>()
                .HasIndex(u => u.Login);

            modelBuilder.Entity<Gift>()
                .HasPartitionKey(g => g.UserId)
                .HasNoDiscriminator()
                .ToContainer("Gifts");

            modelBuilder.Entity<Gift>()
                .OwnsOne(g => g.Card);

            modelBuilder.Entity<Gift>()
                .OwnsOne(g => g.Ornament);

            base.OnModelCreating(modelBuilder);
        }
    }
}
