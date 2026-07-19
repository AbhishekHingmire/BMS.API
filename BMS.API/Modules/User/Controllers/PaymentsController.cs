using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.Services;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using BMS.API.Modules.Shared.Services;
using BMS.API.Modules.User.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.User.Controllers
{
    /// <summary>
    /// Handles the online-payment leg of a booking that's already been created (Unpaid, holding
    /// the seat) via <see cref="UserBookingsController.CreateBooking"/>. Cashfree is the payment
    /// processor; the actual "it's paid" source of truth is the webhook
    /// (see CashfreeWebhookController), this controller's status endpoint is only a fallback for
    /// when the frontend returns from checkout before the webhook has landed.
    /// </summary>
    [ApiController]
    [Route("api/user/bookings/{id}/payment")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICashfreeService _cashfree;
        private readonly INotificationRuleEngine _ruleEngine;

        public PaymentsController(ApplicationDbContext context, ICashfreeService cashfree, INotificationRuleEngine ruleEngine)
        {
            _context = context;
            _cashfree = cashfree;
            _ruleEngine = ruleEngine;
        }

        private async Task<(Guid userId, Booking? booking)> LoadOwnedBookingAsync(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return (Guid.Empty, null);

            var booking = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            return (userId, booking);
        }

        /// <summary>
        /// Creates (or resumes) a Cashfree order for this booking and returns the
        /// payment_session_id the frontend's Cashfree Checkout widget needs.
        /// </summary>
        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder(Guid id, [FromBody] CreatePaymentOrderDto dto)
        {
            var (_, booking) = await LoadOwnedBookingAsync(id);
            if (booking == null) return NotFound();

            if (booking.PaymentMethod != PaymentMethod.OnlinePrepay)
                return BadRequest(new { message = "This booking isn't set up for online payment." });

            if (booking.PaymentStatus == PaymentStatus.Paid)
                return BadRequest(new { message = "This booking is already paid." });

            if (booking.Status == BookingStatus.Cancelled)
                return BadRequest(new { message = "This booking was cancelled." });

            // The booking's own id doubles as the Cashfree order id - it's already unique, and
            // re-creating an order for a still-ACTIVE order id is idempotent on Cashfree's side,
            // so retrying (e.g. user re-opens the payment dialog) safely resumes the same order.
            var orderId = booking.Id.ToString();
            var returnUrl = string.IsNullOrWhiteSpace(dto?.ReturnUrl)
                ? $"{Request.Scheme}://{Request.Host}/confirmation/{booking.Id}"
                : dto!.ReturnUrl!;
            var notifyUrl = $"{Request.Scheme}://{Request.Host}/api/webhooks/cashfree";

            var result = await _cashfree.CreateOrderAsync(
                orderId,
                booking.Price,
                booking.UserId?.ToString() ?? orderId,
                booking.User?.PhoneNumber ?? booking.StudentContact,
                booking.User?.Email,
                returnUrl,
                notifyUrl);

            if (!result.Success)
                return StatusCode(502, new { message = result.ErrorMessage ?? "Could not start the payment. Please try again." });

            return Ok(new { paymentSessionId = result.PaymentSessionId, orderId = result.OrderId });
        }

        /// <summary>
        /// Fallback reconciliation check: asks Cashfree directly for this order's status and
        /// updates the booking if it's actually paid but our webhook hasn't processed it yet
        /// (network delay, webhook retry backlog, etc.). Safe to call repeatedly (idempotent).
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var (_, booking) = await LoadOwnedBookingAsync(id);
            if (booking == null) return NotFound();

            if (booking.PaymentStatus == PaymentStatus.Paid)
                return Ok(new { paymentStatus = "paid" });

            var orderId = booking.Id.ToString();
            var result = await _cashfree.GetOrderStatusAsync(orderId);
            if (!result.Success)
                return Ok(new { paymentStatus = booking.PaymentStatus.ToString().ToLower(), reconciled = false });

            if (string.Equals(result.OrderStatus, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                var becamePaid = booking.PaymentStatus != PaymentStatus.Paid;
                booking.PaymentStatus = PaymentStatus.Paid;
                booking.PaymentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (becamePaid)
                    await _ruleEngine.ProcessBookingPaymentUpdatedAsync(booking);

                return Ok(new { paymentStatus = "paid" });
            }

            return Ok(new { paymentStatus = booking.PaymentStatus.ToString().ToLower(), cashfreeOrderStatus = result.OrderStatus });
        }
    }
}
