using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/libraries/{libraryId}/enquiries")]
    [Authorize(Roles = "Owner,Manager")]
    public class OwnerEnquiriesController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;
        private readonly ApplicationDbContext _context;

        public OwnerEnquiriesController(ILibraryManagementService libraryService, ApplicationDbContext context)
        {
            _libraryService = libraryService;
            _context = context;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        private static EnquiryDto ToDto(Enquiry e) => new EnquiryDto
        {
            Id = e.Id,
            LibraryId = e.LibraryId,
            StudentName = e.StudentName,
            StudentContact = e.StudentContact,
            StudentEmail = e.StudentEmail,
            Gender = e.Gender,
            Notes = e.Notes,
            Status = e.Status,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        [HttpGet]
        public async Task<IActionResult> GetEnquiries(Guid libraryId)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var enquiries = await _context.Enquiries
                .Where(e => e.LibraryId == libraryId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(enquiries.Select(ToDto));
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnquiry(Guid libraryId, [FromBody] CreateEnquiryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (request.LibraryId != libraryId)
            {
                return BadRequest(new { message = "Library ID in path does not match payload." });
            }
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var enquiry = new Enquiry
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                StudentName = request.StudentName,
                StudentContact = request.StudentContact,
                StudentEmail = request.StudentEmail,
                Gender = request.Gender,
                Notes = request.Notes,
                Status = EnquiryStatus.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Enquiries.Add(enquiry);
            await _context.SaveChangesAsync();

            return Ok(ToDto(enquiry));
        }

        [HttpPut("{enquiryId}/status")]
        public async Task<IActionResult> UpdateStatus(Guid libraryId, Guid enquiryId, [FromBody] UpdateEnquiryStatusDto request)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var enquiry = await _context.Enquiries.FirstOrDefaultAsync(e => e.Id == enquiryId && e.LibraryId == libraryId);
            if (enquiry == null) return NotFound();

            enquiry.Status = request.Status;
            enquiry.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ToDto(enquiry));
        }

        [HttpDelete("{enquiryId}")]
        public async Task<IActionResult> DeleteEnquiry(Guid libraryId, Guid enquiryId)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var enquiry = await _context.Enquiries.FirstOrDefaultAsync(e => e.Id == enquiryId && e.LibraryId == libraryId);
            if (enquiry == null) return NotFound();

            _context.Enquiries.Remove(enquiry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enquiry deleted." });
        }
    }
}
