using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/bookings")]
    // [Authorize(Roles = "Owner,Manager")]
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
            // Blueprint
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(Guid libraryId)
        {
            // Blueprint
            return Ok();
        }
    }
}
