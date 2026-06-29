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
        public string PhotosJson { get; set; } // JSON array of photo URLs
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public System.Collections.Generic.List<string> Photos 
        { 
            get => string.IsNullOrEmpty(PhotosJson) ? new System.Collections.Generic.List<string>() : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(PhotosJson); 
            set => PhotosJson = System.Text.Json.JsonSerializer.Serialize(value ?? new System.Collections.Generic.List<string>()); 
        }

        public bool IsVerified { get; set; }
        public bool IsPublished { get; set; }
        public string CancellationPolicy { get; set; }
        public string FaqJson { get; set; } // JSON array of {question, answer}
        
        // Navigation properties
        public ICollection<Area> Areas { get; set; } = new List<Area>();
        public ICollection<ShiftTemplate> Shifts { get; set; } = new List<ShiftTemplate>();
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
