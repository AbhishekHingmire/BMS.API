using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    public class Seat
    {
        public Guid Id { get; set; }
        public Guid AreaId { get; set; }
        
        [MaxLength(50)]
        public string Number { get; set; }
        public GenderRestriction GenderRestriction { get; set; } = GenderRestriction.None;
        public decimal? PriceOverride { get; set; }
        public bool IsInactive { get; set; }

        // Soft-delete flag. Seats referenced by any booking (even past/cancelled ones)
        // can't be hard-deleted due to the FK constraint on Bookings.SeatId, so removal
        // from the owner UI marks the row deleted instead and it is excluded from every
        // read path (booking, browse, owner seat lists).
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        public Area Area { get; set; }
    }
}
