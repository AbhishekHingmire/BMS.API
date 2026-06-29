using System;
using System.Collections.Generic;

namespace BMS.API.Modules.Shared.Models
{
    public class Area
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public string Name { get; set; }
        public string TagsString { get; set; } // Comma separated amenities
        
        public PriceModifierType? PriceModifierType { get; set; }
        public decimal? PriceModifierValue { get; set; }
        
        public Guid? ShiftOverrideId { get; set; }

        // Floor plan could be stored as JSON string to keep DB simple for now
        public string FloorPlanJson { get; set; }
        
        // Navigation properties
        public Library Library { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
