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
        
        // Navigation properties
        public Area Area { get; set; }
    }
}
