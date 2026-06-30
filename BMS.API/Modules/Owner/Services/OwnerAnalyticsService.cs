using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Owner.Services
{
    public class OwnerAnalyticsService : IOwnerAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public OwnerAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OwnerAnalyticsDto> GetAnalyticsAsync(Guid ownerId, Guid? libraryId = null)
        {
            var librariesQuery = _context.Libraries.Where(l => l.OwnerId == ownerId);
            if (libraryId.HasValue)
            {
                librariesQuery = librariesQuery.Where(l => l.Id == libraryId.Value);
            }

            var libraryIds = await librariesQuery.Select(l => l.Id).ToListAsync();
            
            // Note: In a real app we'd aggregate in SQL. For simplicity and since we use in-memory EF often,
            // we will pull bookings for these libraries.
            var allBookings = await _context.Bookings
                .Include(b => b.Plan)
                .Include(b => b.Shift)
                .Include(b => b.Library)
                .Include(b => b.User)
                .Where(b => libraryIds.Contains(b.LibraryId) && !b.IsDeactivated)
                .ToListAsync();

            var bookings = allBookings.Where(b => b.Status != BookingStatus.Cancelled).ToList();

            // Total seats across all requested libraries
            var totalSeats = await _context.Areas
                .Where(a => libraryIds.Contains(a.LibraryId))
                .SelectMany(a => a.Seats)
                .CountAsync();

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-7);

            var dto = new OwnerAnalyticsDto();
            
            var paidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded).ToList();
            var unpaidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Unpaid).ToList();

            dto.TotalBookingsCount = bookings.Count;
            dto.ActiveBookingsCount = bookings.Count(b => b.StartDate <= today && b.EndDate >= today);
            
            var currentTime = DateTime.UtcNow.TimeOfDay;
            var activeBookingsToday = bookings.Where(b => b.StartDate <= today && b.EndDate >= today && b.Status != BookingStatus.Expired).ToList();
            
            dto.OccupiedNowCount = activeBookingsToday.Count(b => b.Shift != null && b.Shift.StartTime <= currentTime && b.Shift.EndTime >= currentTime);
            
            var latestBookings = bookings
                .GroupBy(b => b.UserId?.ToString() ?? (!string.IsNullOrEmpty(b.StudentContact) ? b.StudentContact : (b.StudentName ?? "unknown")))
                .Select(g => g.OrderByDescending(x => x.EndDate).First())
                .ToList();

            var expiringBookings = latestBookings.Where(b => (b.Status == BookingStatus.Active) && (b.EndDate - today).TotalDays >= -1 && (b.EndDate - today).TotalDays <= 7).ToList();
            
            Console.WriteLine($"[DEBUG] Total Latest Bookings: {latestBookings.Count}");
            foreach (var b in latestBookings)
            {
                var diff = (b.EndDate - today).TotalDays;
                Console.WriteLine($"[DEBUG] Booking ID: {b.Id}, Name: {b.StudentName}, Status: {b.Status}, EndDate: {b.EndDate}, DiffDays: {diff}");
            }
            
            dto.ExpiringSoonCount = expiringBookings.Count;
            dto.ExpiringMemberships = expiringBookings.Select(b => new ExpiringMembershipDto
            {
                Id = b.Id,
                StudentName = b.StudentName ?? "Unknown",
                StudentContact = b.StudentContact ?? "",
                LibraryName = b.Library?.Name ?? "Unknown",
                EndDate = b.EndDate.ToString("yyyy-MM-dd"),
                PlanName = b.Plan?.Duration.ToString() ?? "Custom"
            }).ToList();
            
            var monthStart = today.AddDays(-30);

            dto.TodaysRevenue = paidBookings.Where(b => b.CreatedAt.Date == today).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
            dto.SevenDaysRevenue = paidBookings.Where(b => b.CreatedAt.Date >= weekStart).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
            dto.ThirtyDaysRevenue = paidBookings.Where(b => b.CreatedAt.Date >= monthStart).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
            
            dto.PendingCollectionCount = unpaidBookings.Count;
            dto.PendingCollectionAmount = unpaidBookings.Sum(b => b.Price - (b.RefundedAmount ?? 0m));

            // Daily metrics for 30 days
            for (int i = 29; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                var dayPaid = paidBookings.Where(b => b.CreatedAt.Date == d).ToList();
                var online = dayPaid.Where(b => b.PaymentMethod == PaymentMethod.OnlinePrepay).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
                var offline = dayPaid.Where(b => b.PaymentMethod == PaymentMethod.PayAtLibrary).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
                
                var activeThatDay = bookings.Where(b => b.StartDate <= d && b.EndDate >= d).Select(b => b.SeatId).Distinct().Count();
                var occupancy = totalSeats > 0 ? (int)Math.Round((double)activeThatDay / totalSeats * 100) : 0;

                dto.DailyMetrics.Add(new DailyMetricDto
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    Label = d.ToString("dd MMM"),
                    Online = online,
                    Offline = offline,
                    Total = online + offline,
                    Occupancy = occupancy
                });
            }

            dto.PaymentMethodSplit.Add(new PaymentMethodSplitDto 
            { 
                Name = "Online prepay", 
                Value = paidBookings.Count(b => b.PaymentMethod == PaymentMethod.OnlinePrepay)
            });
            dto.PaymentMethodSplit.Add(new PaymentMethodSplitDto 
            { 
                Name = "Pay at desk", 
                Value = paidBookings.Count(b => b.PaymentMethod == PaymentMethod.PayAtLibrary)
            });

            var planGroups = bookings.GroupBy(b => b.Plan != null ? b.Plan.Duration.ToString() : "unknown");
            foreach(var g in planGroups)
            {
                dto.PlanPopularity.Add(new PlanPopularityDto
                {
                    Name = g.Key,
                    Value = g.Count()
                });
            }

            return dto;
        }
    }
}
