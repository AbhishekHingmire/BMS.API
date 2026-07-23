using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;

namespace BMS.API.Modules.User.Controllers
{
    [ApiController]
    [Route("api/user/settings")]
    [Authorize(Roles = "User")]
    public class UserSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var user = await _context.EndUsers.FindAsync(GetUserId());
            if (user == null) return NotFound();

            return Ok(new
            {
                Contact = user.PhoneNumber,
                City = user.City,
                Locality = user.Locality,
                EmailNotif = true,
                SmsNotif = true,
                PromoNotif = false
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsDto request)
        {
            var user = await _context.EndUsers.FindAsync(GetUserId());
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Contact))
                user.PhoneNumber = request.Contact;
            if (!string.IsNullOrEmpty(request.City))
                user.City = request.City;
            if (!string.IsNullOrEmpty(request.Locality))
                user.Locality = request.Locality;

            await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("device-token")]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] UserDeviceTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token)) return BadRequest("Token is required");

            var user = await _context.EndUsers.FindAsync(GetUserId());
            if (user == null) return NotFound();

            user.FcmToken = request.Token;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class UserDeviceTokenRequest
    {
        public string Token { get; set; }
    }

    public class UpdateUserSettingsDto
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string City { get; set; }
        public string Locality { get; set; }
        public bool EmailNotif { get; set; }
        public bool SmsNotif { get; set; }
        public bool PromoNotif { get; set; }
    }
}
