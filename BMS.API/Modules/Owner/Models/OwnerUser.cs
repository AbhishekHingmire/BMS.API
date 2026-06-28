using System;

namespace BMS.API.Modules.Owner.Models
{
    public class OwnerUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        public string PasswordHash { get; set; }
        
        public OwnerRole Role { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum OwnerRole
    {
        Owner,
        Manager
    }
}
