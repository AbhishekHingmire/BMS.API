using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Owner.Models;
using BMS.API.Modules.SuperAdmin.DTOs;

namespace BMS.API.Modules.SuperAdmin.Controllers
{
    [ApiController]
    [Route("api/superadmin/owners")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOwners([FromQuery] string nameOrMobile = null, [FromQuery] string city = null)
        {
            var query = _context.OwnerUsers.AsQueryable();

            if (!string.IsNullOrEmpty(nameOrMobile))
            {
                var search = nameOrMobile.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(search) || o.PhoneNumber.Contains(search));
            }

            var owners = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            var ownerIds = owners.Select(o => o.Id).ToList();
            var libraries = await _context.Libraries
                .Where(l => ownerIds.Contains(l.OwnerId))
                .ToListAsync();

            if (!string.IsNullOrEmpty(city))
            {
                var citySearch = city.ToLower();
                var matchingOwnerIds = libraries.Where(l => l.City != null && l.City.ToLower().Contains(citySearch))
                                                .Select(l => l.OwnerId).Distinct().ToList();
                owners = owners.Where(o => matchingOwnerIds.Contains(o.Id)).ToList();
            }

            var result = owners.Select(o => new 
            {
                o.Id,
                o.Name,
                o.PhoneNumber,
                o.Email,
                o.CreatedAt,
                o.Plan,
                LibrariesCount = libraries.Count(l => l.OwnerId == o.Id),
                LastLogin = o.CreatedAt // Mocking last login as created at for now
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnerDetails(Guid id)
        {
            var owner = await _context.OwnerUsers.FindAsync(id);
            if (owner == null) return NotFound();

            var libraries = await _context.Libraries
                .Include(l => l.Plans)
                .Include(l => l.Bookings)
                .ThenInclude(b => b.User)
                .Include(l => l.Areas)
                .ThenInclude(a => a.Seats)
                .Where(l => l.OwnerId == id)
                .ToListAsync();

            var libraryDetails = libraries.Select(l => new 
            {
                l.Id,
                l.Name,
                l.City,
                TotalSeats = l.Areas.SelectMany(a => a.Seats).Count(),
                BookedSeats = l.Bookings.Count(b => b.Status == BMS.API.Modules.Shared.Models.BookingStatus.Active),
                TotalRevenue = l.Bookings.Where(b => b.PaymentStatus == BMS.API.Modules.Shared.Models.PaymentStatus.Paid).Sum(b => b.Price),
                
                Plans = l.Plans.Where(p => !p.IsDeleted).Select(p => new {
                    p.Id,
                    p.Name,
                    p.BasePrice,
                    Duration = p.Duration.ToString()
                }),
                
                Areas = l.Areas.Where(a => !a.IsDeleted).Select(a => new {
                    a.Id,
                    a.Name,
                    TotalSeats = a.Seats.Count(s => !s.IsDeleted),
                    BookedSeats = l.Bookings.Count(b => b.AreaId == a.Id && b.Status == BMS.API.Modules.Shared.Models.BookingStatus.Active),
                    VacantSeats = a.Seats.Count(s => !s.IsDeleted) - l.Bookings.Count(b => b.AreaId == a.Id && b.Status == BMS.API.Modules.Shared.Models.BookingStatus.Active)
                }),

                Bookings = l.Bookings.Select(b => {
                    var seatNumber = b.SeatId != null ? l.Areas.SelectMany(a => a.Seats).FirstOrDefault(s => s.Id == b.SeatId)?.Number : null;
                    return new {
                        b.Id,
                        b.UserId,
                        UserName = b.User != null ? b.User.Name : "Unknown",
                        UserPhone = b.User != null ? b.User.PhoneNumber : "Unknown",
                        Status = b.Status.ToString(),
                        PaymentStatus = b.PaymentStatus.ToString(),
                        SeatNumber = seatNumber,
                        b.StartDate,
                        b.EndDate,
                        b.Price
                    };
                }).OrderByDescending(b => b.StartDate).ToList()
            });

            return Ok(new 
            {
                owner.Id,
                owner.Name,
                owner.PhoneNumber,
                owner.Email,
                owner.Plan,
                owner.CreatedAt,
                TotalRevenue = libraryDetails.Sum(l => l.TotalRevenue),
                TotalUsers = libraryDetails.SelectMany(l => l.Bookings).Select(b => b.UserId).Distinct().Count(),
                TotalBookings = libraryDetails.SelectMany(l => l.Bookings).Count(),
                Libraries = libraryDetails
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOwner(Guid id, [FromBody] SuperAdminUpdateOwnerDto dto)
        {
            var owner = await _context.OwnerUsers.FindAsync(id);
            if (owner == null) return NotFound();

            owner.Name = dto.Name;
            owner.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                owner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(owner);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOwner(Guid id)
        {
            var owner = await _context.OwnerUsers.FindAsync(id);
            if (owner == null) return NotFound();

            var libraries = await _context.Libraries.Where(l => l.OwnerId == id).ToListAsync();
            _context.Libraries.RemoveRange(libraries);
            _context.OwnerUsers.Remove(owner);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/allocate-plan")]
        public async Task<IActionResult> AllocatePlan(Guid id, [FromBody] SuperAdminAllocatePlanDto dto)
        {
            var owner = await _context.OwnerUsers.FindAsync(id);
            if (owner == null) return NotFound();

            owner.Plan = dto.Plan.ToLowerInvariant();
            owner.PlanStartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(owner);
        }

        [HttpPost("{id}/notify")]
        public async Task<IActionResult> NotifyOwner(Guid id, [FromBody] SuperAdminNotifyDto dto)
        {
            var owner = await _context.OwnerUsers.FindAsync(id);
            if (owner == null) return NotFound();

            var notification = new OwnerNotification
            {
                Id = Guid.NewGuid(),
                OwnerId = id,
                Title = dto.Title,
                Body = dto.Description,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.OwnerNotifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(notification);
        }
    }
}
