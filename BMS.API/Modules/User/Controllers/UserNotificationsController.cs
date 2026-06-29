using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;

namespace BMS.API.Modules.User.Controllers
{
    [ApiController]
    [Route("api/user/notifications")]
    [Authorize(Roles = "User")]
    public class UserNotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserNotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Body,
                    date = n.CreatedAt.ToString("o"),
                    isRead = n.IsRead
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead()
        {
            var userId = GetUserId();
            var unread = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            if (unread.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Notifications marked as read" });
        }
    }
}
