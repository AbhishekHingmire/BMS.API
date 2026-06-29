using System;

namespace BMS.API.Modules.User.Models
{
    public class UserNotification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? BroadcastId { get; set; }
        
        public EndUser User { get; set; }
    }
}
