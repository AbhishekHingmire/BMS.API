using System;

namespace BMS.API.Modules.Shared.DTOs
{
    /// <summary>Response from a "share receipt" endpoint - the frontend builds the full public
    /// URL itself as `${origin}/receipt/{token}`.</summary>
    public class ShareReceiptResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>Flattened, denormalized receipt shape returned by the public (unauthenticated)
    /// receipt endpoint. Mirrors the shape the frontend already builds client-side for the
    /// owner/student receipt dialog (see receipt-dialog.tsx / owner.payments.tsx).</summary>
    public class PublicReceiptDto
    {
        public string Code { get; set; }
        public string StudentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal Price { get; set; }
        public string PaymentStatus { get; set; }

        public string LibraryName { get; set; }
        public string LibraryArea { get; set; }
        public string LibraryCity { get; set; }

        public string AreaName { get; set; }
        public string SeatName { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan ShiftStartTime { get; set; }
        public TimeSpan ShiftEndTime { get; set; }
    }
}
