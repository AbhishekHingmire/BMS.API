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

        // Soft-delete flag. Shifts referenced by any booking can't be hard-deleted due to
        // the FK constraint on Bookings.ShiftId, so removal from the owner UI marks the
        // row deleted instead and it is excluded from every read path.
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        public Library Library { get; set; }
    }
}
