using System;
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

        [HttpPost]
        public async Task<IActionResult> AddSeat(Guid areaId, [FromBody] SeatCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
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
            var seats = await _libraryService.GetSeatsAsync(areaId);
            return Ok(seats);
        }

        [HttpPut("{seatId}")]
        public async Task<IActionResult> UpdateSeat(Guid areaId, Guid seatId, [FromBody] SeatCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var seat = await _libraryService.UpdateSeatAsync(seatId, request);
            if (seat == null) return NotFound("Seat not found.");
            
            return Ok(seat);
        }

        [HttpDelete("{seatId}")]
        public async Task<IActionResult> DeleteSeat(Guid areaId, Guid seatId)
        {
            var success = await _libraryService.DeleteSeatAsync(seatId);
            if (!success) return NotFound("Seat not found.");
            
            return NoContent();
        }

        [HttpPatch("{seatId}/toggle-restrict")]
        public async Task<IActionResult> ToggleSeatRestriction(Guid areaId, Guid seatId)
        {
            var seat = await _libraryService.ToggleSeatRestrictionAsync(seatId);
            if (seat == null) return NotFound("Seat not found.");
            
            return Ok(seat);
        }
    }
}
