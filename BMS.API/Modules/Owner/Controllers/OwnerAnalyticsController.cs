using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/analytics")]
    [Authorize(Roles = "Owner,Manager")]
    public class OwnerAnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OwnerAnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetAnalytics([FromQuery] Guid? libraryId)
        {
            var ownerId = GetOwnerId();
            
            // Get libraries for the owner
            var librariesQuery = _context.Libraries
                .Include(l => l.Areas).ThenInclude(a => a.Seats)
                .Where(l => l.OwnerId == ownerId);
                
            if (libraryId.HasValue)
            {
                librariesQuery = librariesQuery.Where(l => l.Id == libraryId.Value);
            }
            
            var libraries = await librariesQuery.ToListAsync();
            var libraryIds = libraries.Select(l => l.Id).ToList();
            
            if (!libraryIds.Any()) 
            {
                return Ok(new
                {
                    revenueTotal = 0,
                    revenueGrowth = 0,
                    occupancyPercent = 0,
                    activeBookings = 0,
                    pendingCollectionAmount = 0,
                    pendingCollectionCount = 0,
                    dailyMetrics = new List<object>(),
                    revenueByShift = new List<object>()
                });
            }

            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = today.AddDays(-30);

            var bookings = await _context.Bookings
                .Include(b => b.Library)
                .Include(b => b.Shift)
                .Include(b => b.Plan)
                .Where(b => libraryIds.Contains(b.LibraryId))
                .ToListAsync();

            // Calculate total seats
            var totalSeats = libraries.SelectMany(l => l.Areas).SelectMany(a => a.Seats).Count();
            if (totalSeats == 0) totalSeats = 1; // Prevent division by zero

            // Calculate Active Bookings today
            var activeBookingsList = bookings.Where(b => b.Status != BookingStatus.Cancelled && 
                                                         b.Status != BookingStatus.Expired &&
                                                         b.StartDate.Date <= today &&
                                                         b.EndDate.Date >= today).ToList();
            var activeBookings = activeBookingsList.Count;
            var occupiedNowCount = activeBookingsList.Select(b => b.SeatId).Distinct().Count();
            
            // Occupancy Percent
            var occupancyPercent = (int)Math.Round((double)occupiedNowCount / totalSeats * 100);

            // Pending Collections (pay-at-library and not paid)
            var pendingCollections = bookings.Where(b => b.PaymentStatus != PaymentStatus.Paid && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired).ToList();
            var pendingCollectionCount = pendingCollections.Count;
            var pendingCollectionAmount = pendingCollections.Sum(b => b.Price);

            // Daily Metrics (last 30 days)
            var dailyMetrics = new List<object>();
            for (int i = 29; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateString = date.ToString("yyyy-MM-dd");
                
                var dayBookings = bookings.Where(b => 
                    b.StartDate.Date <= date &&
                    b.EndDate.Date >= date &&
                    b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired).ToList();

                var dayRevenueBookings = bookings.Where(b => b.CreatedAt.Date == date && (b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded)).ToList();

                var dayOnline = dayRevenueBookings.Where(b => b.Source == BookingSource.Online).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
                var dayOffline = dayRevenueBookings.Where(b => b.Source == BookingSource.Offline).Sum(b => b.Price - (b.RefundedAmount ?? 0m));
                var dayOccupancy = (int)Math.Round((double)dayBookings.Count / totalSeats * 100);

                dailyMetrics.Add(new
                {
                    label = date.ToString("MMM dd"),
                    date = dateString,
                    online = dayOnline,
                    offline = dayOffline,
                    total = dayOnline + dayOffline,
                    occupancy = dayOccupancy
                });
            }

            // Seven Days Revenue
            var sevenDaysRevenue = bookings
                .Where(b => (b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded) && b.CreatedAt.Date >= today.AddDays(-7))
                .Sum(b => b.Price - (b.RefundedAmount ?? 0m));

            // Today's Revenue (paid/refunded bookings actually created today, not the all-time total)
            var todaysRevenue = bookings
                .Where(b => (b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded) && b.CreatedAt.Date == today)
                .Sum(b => b.Price - (b.RefundedAmount ?? 0m));

            // Total Revenue (all time paid)
            var revenueTotal = bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded)
                .Sum(b => b.Price - (b.RefundedAmount ?? 0m));

            // Revenue By Shift
            var revenueByShift = bookings
                .Where(b => (b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded) && b.Shift != null)
                .GroupBy(b => b.Shift.Name)
                .Select(g => new { name = g.Key, value = g.Sum(b => b.Price - (b.RefundedAmount ?? 0m)) })
                .ToList();

            // Payment Method Split
            var paymentMethodSplit = new List<object>
            {
                new { name = "Online prepay", value = bookings.Where(b => b.PaymentMethod == PaymentMethod.OnlinePrepay && (b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded)).Sum(b => b.Price - (b.RefundedAmount ?? 0m)) },
                new { name = "Pay at desk", value = bookings.Where(b => b.PaymentMethod == PaymentMethod.PayAtLibrary && (b.PaymentStatus == PaymentStatus.Paid || b.PaymentStatus == PaymentStatus.Refunded)).Sum(b => b.Price - (b.RefundedAmount ?? 0m)) }
            };

            // Plan Popularity
            var planPopularity = bookings
                .GroupBy(b => b.Plan != null ? b.Plan.Duration.ToString() : "Custom Plan")
                .Select(g => new { name = g.Key, value = g.Count() })
                .ToList();

            return Ok(new
            {
                todaysRevenue,
                sevenDaysRevenue = sevenDaysRevenue,
                totalBookingsCount = bookings.Count,
                activeBookingsCount = activeBookings,
                occupiedNowCount = occupiedNowCount,
                pendingArrivalCount = 0, // mock
                expiringSoonCount = 0, // mock
                expiringMemberships = new List<object>(), // mock
                revenueTotal,
                revenueGrowth = 15, // Dummy growth
                occupancyPercent,
                activeBookings,
                pendingCollectionAmount,
                pendingCollectionCount,
                dailyMetrics,
                revenueByShift,
                paymentMethodSplit,
                planPopularity
            });
        }
    }
}
