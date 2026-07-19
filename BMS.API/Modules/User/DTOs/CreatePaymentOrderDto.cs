namespace BMS.API.Modules.User.DTOs
{
    /// <summary>
    /// Sent by the frontend when it's ready to initiate payment for a booking it just created
    /// (booking already exists server-side, in Unpaid status, holding the seat).
    /// </summary>
    public class CreatePaymentOrderDto
    {
        /// <summary>Where Cashfree should send the browser back to after checkout closes.
        /// Falls back to a sane default (the confirmation page) if not supplied.</summary>
        public string? ReturnUrl { get; set; }
    }
}
