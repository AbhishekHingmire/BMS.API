using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Owner.Models
{
    public class OwnerUser
    {
        public Guid Id { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; }
        
        [MaxLength(100)]
        public string Email { get; set; }
        
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }
        
        public string PasswordHash { get; set; }
        
        public OwnerRole Role { get; set; }

        // Subscription plan: "free" | "starter" | "growth" | "professional" | "enterprise"
        [MaxLength(20)]
        public string Plan { get; set; } = "free";
        public DateTime PlanStartedAt { get; set; } = DateTime.UtcNow;
        
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
        
        [MaxLength(255)]
        public string? FcmToken { get; set; }

        // Marketing notifications (flags only - no SMS/WhatsApp provider wired up yet)
        public bool MarketingEmailsEnabled { get; set; } = false;
        public bool MarketingSmsEnabled { get; set; } = false;
        public bool MarketingWhatsAppEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum OwnerRole
    {
        Owner,
        Manager
    }
}
