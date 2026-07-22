using System;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Shared.Controllers
{
    /// <summary>
    /// Public, unauthenticated endpoint(s) for viewing a shared receipt link. Deliberately has
    /// no [Authorize] - access control is via the unguessable opaque token instead of a login.
    /// </summary>
    [ApiController]
    [Route("api/public/receipts")]
    public class PublicReceiptsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetReceipt(string token)
        {
            var shareToken = await _context.ReceiptShareTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (shareToken == null) return NotFound(new { message = "This receipt link is invalid." });
            if (shareToken.ExpiresAt < DateTime.UtcNow) return NotFound(new { message = "This receipt link has expired." });

            var booking = await _context.Bookings
                .Include(b => b.Library)
                .Include(b => b.Area)
                .Include(b => b.Seat)
                .Include(b => b.Shift)
                .FirstOrDefaultAsync(b => b.Id == shareToken.BookingId);
            if (booking == null) return NotFound(new { message = "Booking not found." });

            var dto = new PublicReceiptDto
            {
                Code = booking.Code,
                StudentName = booking.StudentName,
                CreatedAt = booking.CreatedAt,
                StartDate = booking.StartDate.ToString("yyyy-MM-dd"),
                EndDate = booking.EndDate.ToString("yyyy-MM-dd"),
                Price = booking.Price,
                PaymentStatus = booking.PaymentStatus.ToString().ToLower(),
                LibraryName = booking.Library?.Name,
                LibraryArea = booking.Library?.AreaName,
                LibraryCity = booking.Library?.City,
                AreaName = booking.Area?.Name,
                SeatName = booking.Seat?.Number,
                ShiftName = booking.Shift?.Name,
                ShiftStartTime = booking.Shift?.StartTime ?? TimeSpan.Zero,
                ShiftEndTime = booking.Shift?.EndTime ?? TimeSpan.Zero
            };

            return Ok(dto);
        }
    }
}
