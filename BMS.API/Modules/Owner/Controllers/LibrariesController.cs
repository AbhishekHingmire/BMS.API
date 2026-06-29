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
    [Route("api/owner/libraries")]
    [Authorize(Roles = "Owner,Manager")]
    public class LibrariesController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;

        public LibrariesController(ILibraryManagementService libraryService)
        {
            _libraryService = libraryService;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpPost]
        public async Task<IActionResult> CreateLibrary([FromBody] LibraryCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var library = await _libraryService.CreateLibraryAsync(GetOwnerId(), request);
            return CreatedAtAction(nameof(GetLibrary), new { id = library.Id }, library);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLibrary(Guid id)
        {
            var library = await _libraryService.GetLibraryByIdAsync(id);
            if (library == null || library.OwnerId != GetOwnerId()) return NotFound();

            return Ok(library);
        }

        [HttpGet]
        public async Task<IActionResult> GetOwnerLibraries()
        {
            var libraries = await _libraryService.GetOwnerLibrariesAsync(GetOwnerId());
            return Ok(libraries);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLibrary(Guid id, [FromBody] LibraryUpdateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _libraryService.GetLibraryByIdAsync(id);
            if (existing == null || existing.OwnerId != GetOwnerId()) return NotFound();

            var updated = await _libraryService.UpdateLibraryAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLibrary(Guid id)
        {
            var existing = await _libraryService.GetLibraryByIdAsync(id);
            if (existing == null || existing.OwnerId != GetOwnerId()) return NotFound();

            var result = await _libraryService.DeleteLibraryAsync(id);
            if (!result) return StatusCode(500, "Failed to delete library");

            return NoContent();
        }
    }
}
