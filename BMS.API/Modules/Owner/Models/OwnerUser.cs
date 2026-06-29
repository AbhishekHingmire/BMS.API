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
        
        // Profile
        public string AvatarUrl { get; set; }

        // Billing
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string UpiId { get; set; }
        
        // Notifications
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool SmsNotificationsEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum OwnerRole
    {
        Owner,
        Manager
    }
}
