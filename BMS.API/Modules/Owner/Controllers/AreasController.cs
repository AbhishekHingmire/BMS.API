using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/areas")]
    // [Authorize(Roles = "Owner,Manager")]
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
            // Blueprint
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas(Guid libraryId)
        {
            // Blueprint
            return Ok();
        }
    }
}
