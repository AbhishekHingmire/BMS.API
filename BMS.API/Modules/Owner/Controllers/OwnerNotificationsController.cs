using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using System.Linq;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/notifications")]
    [Authorize(Roles = "Owner,Manager")]
    public class OwnerNotificationsController : ControllerBase
    {
        private readonly IOwnerProfileService _profileService;
        private readonly ApplicationDbContext _context;

        public OwnerNotificationsController(IOwnerProfileService profileService, ApplicationDbContext context)
        {
            _profileService = profileService;
            _context = context;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var ownerId = GetOwnerId();
            var notifications = await _context.OwnerNotifications
                .Where(n => n.OwnerId == ownerId)
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
            var ownerId = GetOwnerId();
            var unread = await _context.OwnerNotifications
                .Where(n => n.OwnerId == ownerId && !n.IsRead)
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

        [HttpGet("rules")]
        public async Task<IActionResult> GetRules()
        {
            var rules = await _profileService.GetRulesAsync(GetOwnerId());
            return Ok(rules);
        }

        [HttpPut("rules")]
        public async Task<IActionResult> UpdateRule([FromBody] UpdateNotificationRuleDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var rule = await _profileService.UpdateRuleAsync(GetOwnerId(), request);
            return Ok(rule);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetBroadcastHistory()
        {
            var history = await _profileService.GetBroadcastHistoryAsync(GetOwnerId());
            return Ok(history);
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var broadcast = await _profileService.CreateBroadcastAsync(GetOwnerId(), request);
            return Ok(broadcast);
        }

        [HttpGet("broadcast/estimate")]
        public async Task<IActionResult> EstimateBroadcast([FromQuery] string libraryId, [FromQuery] string audience)
        {
            var count = await _profileService.GetAudienceCountAsync(GetOwnerId(), libraryId, audience);
            return Ok(new { count });
        }

        [HttpPut("broadcast/{id}")]
        public async Task<IActionResult> UpdateBroadcast(Guid id, [FromBody] BroadcastDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var broadcast = await _profileService.UpdateBroadcastAsync(GetOwnerId(), id, request);
            if (broadcast == null) return NotFound(new { message = "Broadcast not found" });

            return Ok(broadcast);
        }

        [HttpDelete("broadcast/{id}")]
        public async Task<IActionResult> DeleteBroadcast(Guid id)
        {
            var success = await _profileService.DeleteBroadcastAsync(GetOwnerId(), id);
            if (!success) return NotFound(new { message = "Broadcast not found" });

            return Ok(new { message = "Broadcast deleted" });
        }
    }
}
