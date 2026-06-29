using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.User.DTOs
{
    public class OnlineBookingDto
    {
        [Required]
        public Guid LibraryId { get; set; }
        
        [Required]
        public Guid AreaId { get; set; }
        
        [Required]
        public Guid SeatId { get; set; }
        
        [Required]
        public Guid PlanId { get; set; }
        
        [Required]
        public Guid ShiftId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public decimal Price { get; set; }
    }
}
