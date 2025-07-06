using MeterReadingApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadingApi
{
    public class StorageDbContext : DbContext
    {
        public DbSet<MeterReadingAccount> MeterReadings { get; set; }

        public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=MeterReadingStorage.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeterReadingAccount>()
                .HasKey(b => b.AccountId);

            modelBuilder.Entity<MeterReadingAccount>(b =>
            {
                b.Property(e => e.FirstName).IsRequired();
                b.Property(e => e.LastName).IsRequired();
            });
        }
    }
}
