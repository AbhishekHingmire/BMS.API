using System;
using System.ComponentModel.DataAnnotations;
using BMS.API.Modules.Owner.Models;

namespace BMS.API.Modules.Owner.DTOs
{
    public class OwnerRegisterRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        // Email required for owner registration (used as backup/business contact)
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public OwnerRole Role { get; set; }

        // "free" | "starter" | "growth" | "professional" | "enterprise" - defaults to free if omitted
        public string Plan { get; set; }
    }

    public class OwnerLoginRequestDto
    {
        // Owner logs in via phone number
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

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
        public string Plan { get; set; }
        public DateTime PlanStartedAt { get; set; }
    }
}
