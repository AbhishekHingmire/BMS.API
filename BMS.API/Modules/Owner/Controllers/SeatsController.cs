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
    [Route("api/owner/areas/{areaId}/seats")]
    [Authorize(Roles = "Owner,Manager")]
    public class SeatsController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public SeatsController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpPost]
        public async Task<IActionResult> AddSeat(Guid areaId, [FromBody] SeatCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _libraryService.IsAreaOwnedByAsync(areaId, GetOwnerId())) return NotFound();

            try
            {
                var seat = await _libraryService.AddSeatAsync(areaId, request);
                return Ok(seat);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSeats(Guid areaId)
        {
            if (!await _libraryService.IsAreaOwnedByAsync(areaId, GetOwnerId())) return NotFound();

            var seats = await _libraryService.GetSeatsAsync(areaId);
            return Ok(seats);
        }

        [HttpPut("{seatId}")]
        public async Task<IActionResult> UpdateSeat(Guid areaId, Guid seatId, [FromBody] SeatCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _libraryService.IsSeatOwnedByAsync(seatId, GetOwnerId())) return NotFound();

            var seat = await _libraryService.UpdateSeatAsync(seatId, request);
            if (seat == null) return NotFound("Seat not found.");
            
            return Ok(seat);
        }

        [HttpDelete("{seatId}")]
        public async Task<IActionResult> DeleteSeat(Guid areaId, Guid seatId)
        {
            if (!await _libraryService.IsSeatOwnedByAsync(seatId, GetOwnerId())) return NotFound();

            var success = await _libraryService.DeleteSeatAsync(seatId);
            if (!success) return NotFound("Seat not found.");
            
            return NoContent();
        }

        [HttpPatch("{seatId}/toggle-restrict")]
        public async Task<IActionResult> ToggleSeatRestriction(Guid areaId, Guid seatId)
        {
            if (!await _libraryService.IsSeatOwnedByAsync(seatId, GetOwnerId())) return NotFound();

            var seat = await _libraryService.ToggleSeatRestrictionAsync(seatId);
            if (seat == null) return NotFound("Seat not found.");
            
            return Ok(seat);
        }
    }
}
