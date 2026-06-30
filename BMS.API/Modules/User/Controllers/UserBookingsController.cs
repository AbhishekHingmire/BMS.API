using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using BMS.API.Modules.User.DTOs;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BMS.API.Modules.User.Controllers
{
    [ApiController]
    [Route("api/user/bookings")]
    [Authorize]
    public class UserBookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BMS.API.Modules.Owner.Services.INotificationRuleEngine _ruleEngine;

        public UserBookingsController(ApplicationDbContext context, BMS.API.Modules.Owner.Services.INotificationRuleEngine ruleEngine)
        {
            _context = context;
            _ruleEngine = ruleEngine;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var bookings = await _context.Bookings
                .Include(b => b.Library)
                .Include(b => b.Area)
                .Include(b => b.Seat)
                .Include(b => b.Plan)
                .Include(b => b.Shift)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    id = b.Id,
                    code = b.Code,
                    libraryId = b.LibraryId,
                    libraryName = b.Library.Name,
                    libraryCity = b.Library.City,
                    libraryArea = b.Library.AreaName,
                    areaId = b.AreaId,
                    areaName = b.Area.Name,
                    seatId = b.SeatId,
                    seatName = b.Seat.Number,
                    planId = b.PlanId,
                    planName = b.Plan.Name,
                    shiftId = b.ShiftId,
                    shiftName = b.Shift.Name,
                    shiftStartTime = b.Shift.StartTime,
                    shiftEndTime = b.Shift.EndTime,
                    startDate = b.StartDate.ToString("yyyy-MM-dd"),
                    endDate = b.EndDate.ToString("yyyy-MM-dd"),
                    status = b.Status.ToString().ToLower(),
                    paymentStatus = b.PaymentStatus.ToString().ToLower(),
                    price = b.Price,
                    refundedAmount = b.RefundedAmount,
                    createdAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] OnlineBookingDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var user = await _context.EndUsers.FindAsync(userId);
            if (user == null) return Unauthorized();

            // Verify availability
            var isConflict = await _context.Bookings.AnyAsync(b =>
                b.SeatId == dto.SeatId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartDate < dto.EndDate &&
                b.EndDate > dto.StartDate);

            if (isConflict)
            {
                return BadRequest(new { message = "The selected seat is no longer available for these dates." });
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Code = $"BK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                LibraryId = dto.LibraryId,
                AreaId = dto.AreaId,
                SeatId = dto.SeatId,
                PlanId = dto.PlanId,
                ShiftId = dto.ShiftId,
                UserId = userId,
                StudentName = user.Name,
                StudentContact = user.PhoneNumber,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = BookingStatus.PendingArrival,
                Source = BookingSource.Online,
                PaymentMethod = PaymentMethod.OnlinePrepay,
                PaymentStatus = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow,
                ConfirmedArrival = false
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await _ruleEngine.ProcessBookingCreatedAsync(booking);

            return Ok(booking);
        }
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null || booking.UserId != userId)
                return NotFound();

            if (booking.Status == BookingStatus.Cancelled)
                return BadRequest(new { message = "Booking is already cancelled." });

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking cancelled successfully" });
        }
    }
}
