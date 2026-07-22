using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Services;
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

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkIn(Guid libraryId, [FromBody] WalkInBookingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if (request.LibraryId != libraryId) 
            {
                return BadRequest(new { message = "Library ID in path does not match payload." });
            }
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            try
            {
                var booking = await _libraryService.CreateWalkInBookingAsync(request);
                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(Guid libraryId)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var bookings = await _libraryService.GetLibraryBookingsAsync(libraryId);
            return Ok(bookings);
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(Guid libraryId, Guid bookingId, [FromBody] UpdateBookingDto request)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();
            if (!await _libraryService.IsBookingOwnedByAsync(bookingId, GetOwnerId())) return NotFound();

            var booking = await _libraryService.UpdateBookingAsync(bookingId, request);
            if (booking == null) return NotFound("Booking not found.");
            
            return Ok(booking);
        }

        [HttpPut("/api/owner/bookings/{bookingId}/deactivate")]
        public async Task<IActionResult> DeactivateBooking(Guid bookingId, [FromServices] BMS.API.Modules.Shared.Data.ApplicationDbContext context)
        {
            if (!await _libraryService.IsBookingOwnedByAsync(bookingId, GetOwnerId())) return NotFound("Booking not found.");

            var booking = await context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound("Booking not found.");

            booking.IsDeactivated = true;
            await context.SaveChangesAsync();

            return Ok(new { message = "Membership deactivated successfully." });
        }

        [HttpPut("/api/owner/bookings/{bookingId}/reactivate")]
        public async Task<IActionResult> ReactivateBooking(Guid bookingId, [FromServices] BMS.API.Modules.Shared.Data.ApplicationDbContext context)
        {
            if (!await _libraryService.IsBookingOwnedByAsync(bookingId, GetOwnerId())) return NotFound("Booking not found.");

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

        [HttpPost("/api/owner/bookings/{bookingId}/share-receipt")]
        public async Task<IActionResult> ShareReceipt(Guid bookingId, [FromServices] BMS.API.Modules.Shared.Data.ApplicationDbContext context)
        {
            if (!await _libraryService.IsBookingOwnedByAsync(bookingId, GetOwnerId())) return NotFound(new { message = "Booking not found." });

            var result = await ReceiptShareHelper.CreateOrReuseShareTokenAsync(bookingId, context);
            return Ok(result);
        }
    }
}
