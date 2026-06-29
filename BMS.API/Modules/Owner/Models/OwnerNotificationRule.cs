using System;

namespace BMS.API.Modules.Owner.Models
{
    public class OwnerNotificationRule
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        
        // e.g. "expiry", "receipts", "welcome"
        public string RuleType { get; set; } 
        
        public bool IsEnabled { get; set; }
        
        public OwnerUser Owner { get; set; }
    }
}
