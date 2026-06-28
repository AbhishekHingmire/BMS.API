using System;
using System.Collections.Generic;

namespace BMS.API.Modules.Shared.Models
{
    public class Library
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string AreaName { get; set; } // Area/locality name
        public string City { get; set; }
        public string AmenitiesString { get; set; } // Comma separated or JSON
        public bool IsVerified { get; set; }
        public bool IsPublished { get; set; }
        public string CancellationPolicy { get; set; }
        
        // Navigation properties
        public ICollection<Area> Areas { get; set; }
        public ICollection<ShiftTemplate> Shifts { get; set; }
        public ICollection<Plan> Plans { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
