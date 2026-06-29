using System;

namespace BMS.API.Modules.Owner.Models
{
    public class OwnerNotification
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public OwnerUser Owner { get; set; }
    }
}
