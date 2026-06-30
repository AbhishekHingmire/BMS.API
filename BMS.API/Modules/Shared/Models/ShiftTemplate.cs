using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    public class ShiftTemplate
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        // Navigation properties
        public Library Library { get; set; }
    }
}
