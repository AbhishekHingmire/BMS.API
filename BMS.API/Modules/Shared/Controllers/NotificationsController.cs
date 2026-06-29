using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BMS.API.Modules.Shared.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var notifs = await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifs);
        }

        [HttpGet("owner")]
        public async Task<IActionResult> GetOwnerNotifications()
        {
            var ownerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(ownerIdStr, out var ownerId)) return Unauthorized();

            var notifs = await _context.OwnerNotifications
                .Where(n => n.OwnerId == ownerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifs);
        }
        
        [HttpPost("owner/mark-read/{id}")]
        public async Task<IActionResult> MarkOwnerRead(Guid id)
        {
            var notif = await _context.OwnerNotifications.FindAsync(id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost("user/mark-read/{id}")]
        public async Task<IActionResult> MarkUserRead(Guid id)
        {
            var notif = await _context.UserNotifications.FindAsync(id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
