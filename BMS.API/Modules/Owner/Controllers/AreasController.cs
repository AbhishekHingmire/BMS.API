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
    [Route("api/owner/libraries/{libraryId}/areas")]
    [Authorize(Roles = "Owner,Manager")]
    public class AreasController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public AreasController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddArea(Guid libraryId, [FromBody] AreaCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Assuming library ownership validation is done or should be done via a shared helper
            try
            {
                var area = await _libraryService.AddAreaAsync(libraryId, request);
                return Ok(area);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas(Guid libraryId)
        {
            var areas = await _libraryService.GetAreasAsync(libraryId);
            return Ok(areas);
        }

        [HttpPut("{areaId}")]
        public async Task<IActionResult> UpdateArea(Guid libraryId, Guid areaId, [FromBody] AreaCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var area = await _libraryService.UpdateAreaAsync(areaId, request);
            if (area == null) return NotFound("Area not found.");
            
            return Ok(area);
        }

        [HttpDelete("{areaId}")]
        public async Task<IActionResult> DeleteArea(Guid libraryId, Guid areaId)
        {
            var success = await _libraryService.DeleteAreaAsync(areaId);
            if (!success) return NotFound("Area not found.");
            
            return NoContent();
        }
    }
}
