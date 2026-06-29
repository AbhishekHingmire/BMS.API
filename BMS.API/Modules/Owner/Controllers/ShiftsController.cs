using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/shifts")]
    [Authorize(Roles = "Owner,Manager")]
    public class ShiftsController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public ShiftsController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddShift(Guid libraryId, [FromBody] ShiftTemplate request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var shift = await _libraryService.AddShiftAsync(libraryId, request);
                return Ok(shift);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShifts(Guid libraryId)
        {
            var shifts = await _libraryService.GetShiftsAsync(libraryId);
            return Ok(shifts);
        }

        [HttpPut("{shiftId}")]
        public async Task<IActionResult> UpdateShift(Guid libraryId, Guid shiftId, [FromBody] ShiftTemplate request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var shift = await _libraryService.UpdateShiftAsync(shiftId, request);
            if (shift == null) return NotFound("Shift not found.");
            
            return Ok(shift);
        }

        [HttpDelete("{shiftId}")]
        public async Task<IActionResult> DeleteShift(Guid libraryId, Guid shiftId)
        {
            var success = await _libraryService.DeleteShiftAsync(shiftId);
            if (!success) return NotFound("Shift not found.");
            
            return NoContent();
        }
    }
}
