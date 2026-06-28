using BMS.API.Modules.Owner.Models;
using BMS.API.Modules.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Owner Module DbSets
        public DbSet<OwnerUser> OwnerUsers { get; set; }

        // Shared / Core DbSets
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<ShiftTemplate> ShiftTemplates { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Entity Relationships

            modelBuilder.Entity<Library>()
                .HasMany(l => l.Areas)
                .WithOne(a => a.Library)
                .HasForeignKey(a => a.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Library>()
                .HasMany(l => l.Shifts)
                .WithOne(s => s.Library)
                .HasForeignKey(s => s.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Library>()
                .HasMany(l => l.Plans)
                .WithOne(p => p.Library)
                .HasForeignKey(p => p.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Area>()
                .HasMany(a => a.Seats)
                .WithOne(s => s.Area)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Library)
                .WithMany(l => l.Bookings)
                .HasForeignKey(b => b.LibraryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Area)
                .WithMany()
                .HasForeignKey(b => b.AreaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Seat)
                .WithMany()
                .HasForeignKey(b => b.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Plan)
                .WithMany()
                .HasForeignKey(b => b.PlanId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Shift)
                .WithMany()
                .HasForeignKey(b => b.ShiftId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
