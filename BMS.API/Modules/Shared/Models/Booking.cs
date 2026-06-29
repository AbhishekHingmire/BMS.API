using System;

namespace BMS.API.Modules.Shared.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        
        public Guid LibraryId { get; set; }
        public Guid AreaId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PlanId { get; set; }
        public Guid ShiftId { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public BookingStatus Status { get; set; }
        public BookingSource Source { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Price { get; set; }
        public decimal? RefundedAmount { get; set; }
        
        public string StudentName { get; set; }
        public string StudentContact { get; set; }
        public Guid? UserId { get; set; } // Nullable if walk-in without system user
        
        public DateTime CreatedAt { get; set; }
        public bool ConfirmedArrival { get; set; }
        public bool IsDeactivated { get; set; } = false;
        
        // Navigation properties
        public Library Library { get; set; }
        public Area Area { get; set; }
        public Seat Seat { get; set; }
        public Plan Plan { get; set; }
        public ShiftTemplate Shift { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("UserId")]
        public BMS.API.Modules.User.Models.EndUser User { get; set; }
    }
}
