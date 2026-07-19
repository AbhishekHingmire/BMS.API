using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/plans")]
    [Authorize(Roles = "Owner,Manager")]
    public class PlansController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public PlansController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpPost]
        public async Task<IActionResult> AddPlan(Guid libraryId, [FromBody] Plan request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            try
            {
                var plan = await _libraryService.AddPlanAsync(libraryId, request);
                return Ok(plan);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPlans(Guid libraryId)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var plans = await _libraryService.GetPlansAsync(libraryId);
            return Ok(plans);
        }

        [HttpPut("{planId}")]
        public async Task<IActionResult> UpdatePlan(Guid libraryId, Guid planId, [FromBody] Plan request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _libraryService.IsPlanOwnedByAsync(planId, GetOwnerId())) return NotFound();

            var plan = await _libraryService.UpdatePlanAsync(planId, request);
            if (plan == null) return NotFound("Plan not found.");
            
            return Ok(plan);
        }

        [HttpDelete("{planId}")]
        public async Task<IActionResult> DeletePlan(Guid libraryId, Guid planId)
        {
            if (!await _libraryService.IsPlanOwnedByAsync(planId, GetOwnerId())) return NotFound();

            var success = await _libraryService.DeletePlanAsync(planId);
            if (!success) return NotFound("Plan not found.");
            
            return NoContent();
        }
    }
}
