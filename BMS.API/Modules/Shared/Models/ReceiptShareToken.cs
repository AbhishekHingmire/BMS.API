using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    /// <summary>
    /// A shareable, unauthenticated link to a single booking's receipt. Tokens are opaque
    /// random strings (not the booking id) so a receipt can be shared publicly without
    /// exposing/guessing other bookings, and expire after a fixed window.
    /// </summary>
    public class ReceiptShareToken
    {
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        [Required]
        [MaxLength(64)]
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
