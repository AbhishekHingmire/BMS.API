using System.ComponentModel.DataAnnotations;
using BMS.API.Modules.Owner.Models;

namespace BMS.API.Modules.Owner.DTOs
{
    public class OwnerRegisterRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public OwnerRole Role { get; set; }
    }

    public class OwnerLoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
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
