using BMS.API.Modules.Owner.Models;

namespace BMS.API.Modules.Owner.DTOs
{
    public class OwnerRegisterRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public OwnerRole Role { get; set; }
    }

    public class OwnerLoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Message { get; set; }
        public OwnerUserDto User { get; set; }
    }

    public class OwnerUserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
