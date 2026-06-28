using System;

namespace BMS.API.Modules.Shared.Models
{
    public class ShiftTemplate
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        // Navigation properties
        public Library Library { get; set; }
    }
}
