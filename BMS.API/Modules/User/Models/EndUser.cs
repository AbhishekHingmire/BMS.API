using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.User.Models
{
    public class EndUser
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        // Email is optional — used as backup contact only
        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }
        
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [System.Text.Json.Serialization.JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Locality { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Occupation { get; set; }
    }
}
