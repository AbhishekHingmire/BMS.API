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
    // [Authorize(Roles = "Owner,Manager")]
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
            // Blueprint
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetShifts(Guid libraryId)
        {
            // Blueprint
            return Ok();
        }
    }
}
