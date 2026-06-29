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
    [Route("api/owner/account")]
    [Authorize(Roles = "Owner,Manager")]
    public class OwnerAccountController : ControllerBase
    {
        private readonly IOwnerProfileService _profileService;

        public OwnerAccountController(IOwnerProfileService profileService)
        {
            _profileService = profileService;
        }

        private Guid GetOwnerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _profileService.GetProfileAsync(GetOwnerId());
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateOwnerProfileDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var profile = await _profileService.UpdateProfileAsync(GetOwnerId(), request);
            if (profile == null) return NotFound();
            
            return Ok(profile);
        }
    }
}
