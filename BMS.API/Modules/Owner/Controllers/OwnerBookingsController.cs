using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/bookings")]
    [Authorize(Roles = "Owner,Manager")]
    public class OwnerBookingsController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public OwnerBookingsController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkIn(Guid libraryId, [FromBody] WalkInBookingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if (request.LibraryId != libraryId) 
            {
                return BadRequest(new { message = "Library ID in path does not match payload." });
            }

            var booking = await _libraryService.CreateWalkInBookingAsync(request);
            return Ok(booking);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(Guid libraryId)
        {
            var bookings = await _libraryService.GetLibraryBookingsAsync(libraryId);
            return Ok(bookings);
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(Guid libraryId, Guid bookingId, [FromBody] UpdateBookingDto request)
        {
            var booking = await _libraryService.UpdateBookingAsync(bookingId, request);
            if (booking == null) return NotFound("Booking not found.");
            
            return Ok(booking);
        }

        [HttpPut("/api/owner/bookings/{bookingId}/deactivate")]
        public async Task<IActionResult> DeactivateBooking(Guid bookingId, [FromServices] BMS.API.Modules.Shared.Data.ApplicationDbContext context)
        {
            var booking = await context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound("Booking not found.");

            booking.IsDeactivated = true;
            await context.SaveChangesAsync();

            return Ok(new { message = "Membership deactivated successfully." });
        }

        [HttpPut("/api/owner/bookings/{bookingId}/reactivate")]
        public async Task<IActionResult> ReactivateBooking(Guid bookingId, [FromServices] BMS.API.Modules.Shared.Data.ApplicationDbContext context)
        {
            var booking = await context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound("Booking not found.");

            booking.IsDeactivated = false;
            await context.SaveChangesAsync();

            return Ok(new { message = "Membership reactivated successfully." });
        }

        [HttpGet("/api/owner/bookings/verify/{code}")]
        public async Task<IActionResult> VerifyBookingByCode(string code)
        {
            var ownerId = Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var booking = await _libraryService.GetBookingByCodeAsync(ownerId, code);
            
            if (booking == null) 
            {
                return NotFound(new { message = "Booking not found or does not belong to your libraries." });
            }

            return Ok(booking);
        }
    }
}
