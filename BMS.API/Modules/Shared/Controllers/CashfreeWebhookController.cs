using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using BMS.API.Modules.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMS.API.Modules.Shared.Controllers
{
    /// <summary>
    /// Public endpoint Cashfree calls asynchronously to report a payment's final outcome. This
    /// is the actual source of truth for "did the student pay" - never trust the frontend's
    /// "payment succeeded" callback alone, since that's just a UX signal the user's own browser
    /// could fake. Every request here is verified via HMAC signature before anything is trusted.
    /// </summary>
    [ApiController]
    [Route("api/webhooks/cashfree")]
    [AllowAnonymous]
    public class CashfreeWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICashfreeService _cashfree;
        private readonly INotificationRuleEngine _ruleEngine;
        private readonly ILogger<CashfreeWebhookController> _logger;

        public CashfreeWebhookController(
            ApplicationDbContext context,
            ICashfreeService cashfree,
            INotificationRuleEngine ruleEngine,
            ILogger<CashfreeWebhookController> logger)
        {
            _context = context;
            _cashfree = cashfree;
            _ruleEngine = ruleEngine;
            _logger = logger;
        }

        private class WebhookPayload
        {
            [JsonPropertyName("type")] public string? Type { get; set; }
            [JsonPropertyName("data")] public WebhookData? Data { get; set; }
        }

        private class WebhookData
        {
            [JsonPropertyName("order")] public WebhookOrder? Order { get; set; }
            [JsonPropertyName("payment")] public WebhookPayment? Payment { get; set; }
        }

        private class WebhookOrder
        {
            [JsonPropertyName("order_id")] public string? OrderId { get; set; }
        }

        private class WebhookPayment
        {
            [JsonPropertyName("payment_status")] public string? PaymentStatus { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            Request.EnableBuffering();
            string rawBody;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
            }
            Request.Body.Position = 0;

            var timestamp = Request.Headers["x-webhook-timestamp"].ToString();
            var signature = Request.Headers["x-webhook-signature"].ToString();

            if (!_cashfree.VerifyWebhookSignature(timestamp, rawBody, signature))
            {
                _logger.LogWarning("Cashfree webhook signature verification failed.");
                return Unauthorized();
            }

            WebhookPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<WebhookPayload>(rawBody);
            }
            catch (JsonException)
            {
                return BadRequest();
            }

            var orderId = payload?.Data?.Order?.OrderId;
            var paymentStatus = payload?.Data?.Payment?.PaymentStatus;
            if (string.IsNullOrEmpty(orderId) || !Guid.TryParse(orderId, out var bookingId))
            {
                // Not a payment event we recognize (or malformed) - acknowledge so Cashfree
                // doesn't keep retrying, but there's nothing for us to update.
                return Ok();
            }

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return Ok();

            if (string.Equals(paymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                if (booking.PaymentStatus != PaymentStatus.Paid)
                {
                    booking.PaymentStatus = PaymentStatus.Paid;
                    booking.PaymentDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await _ruleEngine.ProcessBookingPaymentUpdatedAsync(booking);
                }
            }
            else if (string.Equals(paymentStatus, "FAILED", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(paymentStatus, "USER_DROPPED", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(paymentStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                // Payment didn't go through - release the seat (only if it hadn't already been
                // paid/confirmed some other way) so another student can book it.
                if (booking.PaymentStatus == PaymentStatus.Unpaid && booking.Status == BookingStatus.Active)
                {
                    booking.Status = BookingStatus.Cancelled;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok();
        }
    }
}
