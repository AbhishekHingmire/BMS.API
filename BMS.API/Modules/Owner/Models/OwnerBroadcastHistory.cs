using System;

namespace BMS.API.Modules.Owner.Models
{
    public class OwnerBroadcastHistory
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Audience { get; set; }
        public string LibraryName { get; set; }
        
        public int EstimatedRecipients { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public OwnerUser Owner { get; set; }
    }
}
