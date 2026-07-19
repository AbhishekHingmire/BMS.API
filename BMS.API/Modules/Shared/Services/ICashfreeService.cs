using System.Threading.Tasks;

namespace BMS.API.Modules.Shared.Services
{
    public class CashfreeOrderResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OrderId { get; set; }
        public string? PaymentSessionId { get; set; }
        public string? OrderStatus { get; set; }
    }

    public class CashfreeOrderStatusResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        /// <summary>Cashfree order_status: ACTIVE, PAID, EXPIRED, TERMINATED, TERMINATION_REQUESTED.</summary>
        public string? OrderStatus { get; set; }
    }

    public interface ICashfreeService
    {
        /// <summary>
        /// Creates (or, if one is already ACTIVE for this order id, re-fetches) a Cashfree order
        /// and returns the payment_session_id the frontend needs to open the checkout widget.
        /// </summary>
        Task<CashfreeOrderResult> CreateOrderAsync(
            string orderId,
            decimal amount,
            string customerId,
            string customerPhone,
            string? customerEmail,
            string returnUrl,
            string notifyUrl);

        /// <summary>Fetches the current order status directly from Cashfree - used as a fallback
        /// reconciliation check when the frontend returns from checkout, in case the async
        /// webhook hasn't landed yet.</summary>
        Task<CashfreeOrderStatusResult> GetOrderStatusAsync(string orderId);

        /// <summary>
        /// Verifies the `x-webhook-signature` header on an incoming Cashfree webhook request by
        /// recomputing HMAC-SHA256(secretKey, timestamp + rawBody) and comparing (constant-time)
        /// to the signature Cashfree sent. Never trust a webhook payload without this check -
        /// anyone could otherwise POST a fake "payment succeeded" event.
        /// </summary>
        bool VerifyWebhookSignature(string timestamp, string rawBody, string signature);
    }
}
