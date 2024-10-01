using Microsoft.EntityFrameworkCore;
using OT.Assessment.App.Models;

namespace OT.Assessment.App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<CasinoWager> CasinoWagers { get; set; }
        public DbSet<Player> Players { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys
            modelBuilder.Entity<Player>()
                .HasKey(p => p.AccountId);

            modelBuilder.Entity<CasinoWager>()
                .HasKey(w => w.WagerId);

            // Specify decimal precision and scale for the Amount property
            modelBuilder.Entity<CasinoWager>()
                .Property(w => w.Amount)
                .HasColumnType("decimal(18, 2)"); // Adjust precision and scale as needed

            // Configure relationships
            modelBuilder.Entity<CasinoWager>()
                .HasOne(w => w.Player)
                .WithMany(p => p.CasinoWagers)
                .HasForeignKey(w => w.AccountId);
        }
    }
}
