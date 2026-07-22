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
    [Route("api/owner/libraries/{libraryId}/attendance")]
    [Authorize(Roles = "Owner,Manager")]
    public class OwnerAttendanceController : ControllerBase
    {
        private readonly ILibraryManagementService _libraryService;
        private readonly ApplicationDbContext _context;

        public OwnerAttendanceController(ILibraryManagementService libraryService, ApplicationDbContext context)
        {
            _libraryService = libraryService;
            _context = context;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        private static AttendanceRecordDto ToDto(AttendanceRecord a) => new AttendanceRecordDto
        {
            Id = a.Id,
            BookingId = a.BookingId,
            LibraryId = a.LibraryId,
            Date = a.Date,
            MarkedAt = a.MarkedAt
        };

        [HttpGet("records")]
        public async Task<IActionResult> GetRecords(Guid libraryId, [FromQuery] int days = 60)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var since = DateTime.UtcNow.Date.AddDays(-Math.Max(1, days));
            var records = await _context.AttendanceRecords
                .Where(a => a.LibraryId == libraryId && a.Date >= since)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return Ok(records.Select(ToDto));
        }

        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance(Guid libraryId, [FromBody] MarkAttendanceDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId && b.LibraryId == libraryId);
            if (booking == null) return NotFound(new { message = "Booking not found in this library." });

            var date = (request.Date ?? DateTime.UtcNow).Date;

            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.BookingId == request.BookingId && a.Date == date);
            if (existing != null) return Ok(ToDto(existing));

            var record = new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                BookingId = request.BookingId,
                LibraryId = libraryId,
                Date = date,
                MarkedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(ToDto(record));
        }

        [HttpDelete("{recordId}")]
        public async Task<IActionResult> UnmarkAttendance(Guid libraryId, Guid recordId)
        {
            if (!await _libraryService.IsLibraryOwnedByAsync(libraryId, GetOwnerId())) return NotFound();

            var record = await _context.AttendanceRecords.FirstOrDefaultAsync(a => a.Id == recordId && a.LibraryId == libraryId);
            if (record == null) return NotFound();

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Attendance unmarked." });
        }
    }
}
