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
            optionsBuilder.UseSqlite(@"Data Source=D:\універ\2 семестр\курсова (ооп)\Drum Machine\drum_machine.db");
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
    }
}