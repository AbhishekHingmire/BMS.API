using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/plans")]
    // [Authorize(Roles = "Owner,Manager")]
    public class PlansController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public PlansController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddPlan(Guid libraryId, [FromBody] Plan request)
        {
            // Blueprint
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPlans(Guid libraryId)
        {
            // Blueprint
            return Ok();
        }
    }
}
