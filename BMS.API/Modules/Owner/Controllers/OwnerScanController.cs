using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/scan")]
    [Authorize]
    public class OwnerScanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OwnerScanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class ScanRequestDto
        {
            public string Code { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> ScanBooking([FromBody] ScanRequestDto dto)
        {
            var ownerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(ownerIdStr, out var ownerId)) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Library)
                .Include(b => b.Area)
                .Include(b => b.Seat)
                .Include(b => b.Plan)
                .Include(b => b.Shift)
                .FirstOrDefaultAsync(b => b.Code == dto.Code);

            if (booking == null)
            {
                return NotFound(new { message = "Booking not found." });
            }

            if (booking.Library.OwnerId != ownerId)
            {
                return Forbid();
            }

            var now = DateTime.UtcNow;
            if (now < booking.StartDate || now > booking.EndDate || booking.Status != BookingStatus.Active)
            {
                return BadRequest(new { message = "Booking is inactive or expired." });
            }

            // Mark attendance or check-in
            booking.ConfirmedArrival = true;
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Scan successful.", 
                booking = new 
                {
                    booking.Id,
                    booking.Code,
                    booking.StudentName,
                    LibraryName = booking.Library.Name,
                    SeatNumber = booking.Seat.Number,
                    ShiftName = booking.Shift.Name,
                    booking.StartDate,
                    booking.EndDate
                }
            });
        }
    }
}
