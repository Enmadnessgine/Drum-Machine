using Microsoft.EntityFrameworkCore;
using Drum_Machine.Data.Entities;

namespace Drum_Machine.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<TrackEntity> Tracks { get; set; }
        public DbSet<SampleEntity> Samples { get; set; }
        public DbSet<UserSettings> Settings { get; set; }
        public DbSet<ExportedTrack> ExportedTracks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=\drum_machine.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Settings)
                .WithOne()
                .HasForeignKey<UserSettings>(s => s.UserId);

            modelBuilder.Entity<ProjectEntity>()
                .HasOne(p => p.ExportedTrack)
                .WithOne()
                .HasForeignKey<ExportedTrack>(e => e.ProjectId);
        }

        public void SeedData()
        {
            Database.EnsureCreated();

            if (!Users.Any(u => u.Username == "Guest"))
            {
                var guest = new User
                {
                    Username = "Guest",
                    Password = "GUEST_SYSTEM_ACCOUNT"
                };
                Users.Add(guest);
                SaveChanges();

                var guestSettings = new UserSettings
                {
                    UserId = guest.Id,
                    Theme = "Dark",
                    MasterVolume = 0.5
                };
                Set<UserSettings>().Add(guestSettings);
                SaveChanges();
            }
        }
    }
}