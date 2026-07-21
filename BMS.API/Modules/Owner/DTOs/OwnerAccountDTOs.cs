using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Owner.DTOs
{
    public class OwnerProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }

        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string UpiId { get; set; }

        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }

        public string Plan { get; set; }
        public DateTime PlanStartedAt { get; set; }
    }

    public class UpdateOwnerProfileDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }

        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string UpiId { get; set; }

        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
    }

    public class NotificationRuleDto
    {
        public Guid Id { get; set; }
        public string RuleType { get; set; }
        public bool IsEnabled { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? BodyTemplate { get; set; }
    }

    public class UpdateNotificationRuleDto
    {
        [Required]
        public string RuleType { get; set; }
        public bool IsEnabled { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? BodyTemplate { get; set; }
    }

    public class BroadcastDto
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public string Audience { get; set; }
        public string LibraryId { get; set; } // "all" or Guid string
        public string LibraryName { get; set; } // Provided from frontend to avoid extra lookup if desired
    }

    public class BroadcastHistoryDto
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Audience { get; set; }
        public string LibraryName { get; set; }
        public int EstimatedRecipients { get; set; }
        public DateTime SentAt { get; set; }
    }
}
