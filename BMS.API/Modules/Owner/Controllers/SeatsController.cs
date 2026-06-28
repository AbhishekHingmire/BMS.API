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
    // [Authorize(Roles = "Owner,Manager")]
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
            // Blueprint
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSeats(Guid areaId)
        {
            // Blueprint
            return Ok();
        }
    }
}
