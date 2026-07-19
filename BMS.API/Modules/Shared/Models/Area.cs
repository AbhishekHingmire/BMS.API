using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    public class Area
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; }
        public string TagsString { get; set; } // Comma separated amenities
        
        public PriceModifierType? PriceModifierType { get; set; }
        public decimal? PriceModifierValue { get; set; }
        
        public Guid? ShiftOverrideId { get; set; }

        // Floor plan could be stored as JSON string to keep DB simple for now
        public string FloorPlanJson { get; set; }

        // Soft-delete flag. Areas can't be hard-deleted once any of their seats have been
        // referenced by a booking (FK constraint), so removal from the owner UI marks the
        // row deleted instead and it is excluded from every read path.
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        public Library Library { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
