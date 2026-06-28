using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Modules.Owner.Controllers
{
    [ApiController]
    [Route("api/owner/auth")]
    public class OwnerAuthController : ControllerBase
    {
        private readonly IOwnerAuthService _authService;

        public OwnerAuthController(IOwnerAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] OwnerRegisterRequestDto request)
        {
            // Blueprint only
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] OwnerLoginRequestDto request)
        {
            // Blueprint only
            return Ok();
        }
    }
}
