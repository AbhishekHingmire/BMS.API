using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Services;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(request);
            if (response.Token == null)
            {
                return Conflict(new { response.Message });
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] OwnerLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);
            if (response.Token == null)
            {
                return Unauthorized(new { response.Message });
            }

            return Ok(response);
        }
    }
}
