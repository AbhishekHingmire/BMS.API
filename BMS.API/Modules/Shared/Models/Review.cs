using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        
        [MaxLength(500)]
        public string Comment { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public Library Library { get; set; }
    }
}
