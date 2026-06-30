using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    public class Plan
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        
        public DurationType Duration { get; set; }
        public Guid ShiftId { get; set; }
        
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountFlat { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string DaysOfWeekString { get; set; } // e.g. "1,2,3,4,5"
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        public Library Library { get; set; }
        public ShiftTemplate Shift { get; set; }
    }
}
