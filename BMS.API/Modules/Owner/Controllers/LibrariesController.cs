using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries")]
    // [Authorize(Roles = "Owner,Manager")] // Commented for blueprint testing
    public class LibrariesController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public LibrariesController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLibrary([FromBody] LibraryCreateDto request)
        {
            // Blueprint
            return CreatedAtAction(nameof(GetLibrary), new { id = Guid.NewGuid() }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLibrary(Guid id)
        {
            // Blueprint
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetOwnerLibraries()
        {
            // Blueprint
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLibrary(Guid id, [FromBody] LibraryUpdateDto request)
        {
            // Blueprint
            return NoContent();
        }
    }
}
