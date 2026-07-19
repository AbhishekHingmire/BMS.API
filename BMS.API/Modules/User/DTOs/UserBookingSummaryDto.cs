using System;

namespace BMS.API.Modules.User.DTOs
{
    /// <summary>
    /// Flattened, student-facing booking shape shared by GET /api/user/bookings (list) and
    /// GET /api/user/bookings/{id} (single). Denormalizes library/area/seat/plan/shift names
    /// so the frontend never needs to fetch the full library collection just to render a
    /// booking card (confirmation, membership, payment history, account pages).
    /// </summary>
    public class UserBookingSummaryDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }
        public string LibraryCity { get; set; }
        public string LibraryArea { get; set; }
        public Guid AreaId { get; set; }
        public string AreaName { get; set; }
        public Guid SeatId { get; set; }
        public string SeatName { get; set; }
        public Guid PlanId { get; set; }
        public string PlanName { get; set; }
        public string PlanDuration { get; set; }
        public Guid ShiftId { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan ShiftStartTime { get; set; }
        public TimeSpan ShiftEndTime { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Price { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
