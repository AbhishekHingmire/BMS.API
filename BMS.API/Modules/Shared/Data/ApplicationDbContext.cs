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
        public DbSet<OwnerNotificationRule> OwnerNotificationRules { get; set; }
        public DbSet<OwnerBroadcastHistory> OwnerBroadcastHistories { get; set; }

        public DbSet<OwnerNotification> OwnerNotifications { get; set; }

        // User Module DbSets
        public DbSet<BMS.API.Modules.User.Models.EndUser> EndUsers { get; set; }
        public DbSet<BMS.API.Modules.User.Models.UserNotification> UserNotifications { get; set; }

        // Shared / Core DbSets
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<ShiftTemplate> ShiftTemplates { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<ReceiptShareToken> ReceiptShareTokens { get; set; }

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

            modelBuilder.Entity<Plan>()
                .HasOne(p => p.Shift)
                .WithMany()
                .HasForeignKey(p => p.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<City>()
                .HasMany(c => c.Localities)
                .WithOne(l => l.City)
                .HasForeignKey(l => l.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data
            var puneId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var mumbaiId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<City>().HasData(
                new City { Id = puneId, Name = "Pune" },
                new City { Id = mumbaiId, Name = "Mumbai" }
            );

            modelBuilder.Entity<Locality>().HasData(
                new Locality { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), CityId = puneId, Name = "Kothrud" },
                new Locality { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), CityId = puneId, Name = "Viman Nagar" },
                new Locality { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), CityId = puneId, Name = "Hinjewadi" },
                new Locality { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), CityId = puneId, Name = "Wakad" },
                new Locality { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), CityId = puneId, Name = "Baner" },
                new Locality { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), CityId = mumbaiId, Name = "Andheri" },
                new Locality { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), CityId = mumbaiId, Name = "Bandra" },
                new Locality { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), CityId = mumbaiId, Name = "Borivali" },
                new Locality { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), CityId = mumbaiId, Name = "Dadar" },
                new Locality { Id = Guid.Parse("20000000-0000-0000-0000-000000000005"), CityId = mumbaiId, Name = "Juhu" }
            );

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

            // Enquiry relationships
            modelBuilder.Entity<Enquiry>()
                .HasOne(e => e.Library)
                .WithMany()
                .HasForeignKey(e => e.LibraryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Attendance: one mark per booking per calendar date
            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.BookingId, a.Date })
                .IsUnique();

            // Receipt share tokens: opaque token must be unique, and we frequently look up
            // "does this booking already have a live token" so index BookingId too.
            modelBuilder.Entity<ReceiptShareToken>()
                .HasIndex(t => t.Token)
                .IsUnique();
            modelBuilder.Entity<ReceiptShareToken>()
                .HasIndex(t => t.BookingId);

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

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OwnerNotification>()
                .HasOne(n => n.Owner)
                .WithMany()
                .HasForeignKey(n => n.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BMS.API.Modules.User.Models.UserNotification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
