using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using BMS.API.Modules.User.DTOs;
using System.Linq;
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BMS.API.Modules.Shared.Services;

namespace BMS.API.Modules.User.Controllers
{
    [ApiController]
    [Route("api/user/bookings")]
    [Authorize]
    public class UserBookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BMS.API.Modules.Owner.Services.INotificationRuleEngine _ruleEngine;

        public UserBookingsController(ApplicationDbContext context, BMS.API.Modules.Owner.Services.INotificationRuleEngine ruleEngine)
        {
            _context = context;
            _ruleEngine = ruleEngine;
        }

        /// <summary>
        /// Shared projection to the flattened, student-facing booking shape. Used by both the
        /// full-list endpoint and the single-booking-by-id endpoint so the two never drift.
        /// </summary>
        private static readonly Expression<Func<Booking, UserBookingSummaryDto>> ToSummaryDto = b => new UserBookingSummaryDto
        {
            Id = b.Id,
            Code = b.Code,
            LibraryId = b.LibraryId,
            LibraryName = b.Library.Name,
            LibraryCity = b.Library.City,
            LibraryArea = b.Library.AreaName,
            AreaId = b.AreaId,
            AreaName = b.Area.Name,
            SeatId = b.SeatId,
            SeatName = b.Seat.Number,
            PlanId = b.PlanId,
            PlanName = b.Plan.Name,
            PlanDuration = b.Plan.Duration.ToString().ToLower(),
            ShiftId = b.ShiftId,
            ShiftName = b.Shift.Name,
            ShiftStartTime = b.Shift.StartTime,
            ShiftEndTime = b.Shift.EndTime,
            StartDate = b.StartDate.ToString("yyyy-MM-dd"),
            EndDate = b.EndDate.ToString("yyyy-MM-dd"),
            Status = b.Status.ToString().ToLower(),
            PaymentStatus = b.PaymentStatus.ToString().ToLower(),
            Price = b.Price,
            RefundedAmount = b.RefundedAmount,
            CreatedAt = b.CreatedAt
        };

        private IQueryable<Booking> BookingsWithIncludes() =>
            _context.Bookings
                .Include(b => b.Library)
                .Include(b => b.Area)
                .Include(b => b.Seat)
                .Include(b => b.Plan)
                .Include(b => b.Shift);

        [HttpGet]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var bookings = await BookingsWithIncludes()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(ToSummaryDto)
                .ToListAsync();

            return Ok(bookings);
        }

        /// <summary>
        /// Fetches a single booking by id, scoped to the authenticated student. Lets pages that
        /// only need one booking (confirmation, membership card) avoid fetching the student's
        /// entire booking history just to find one by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var booking = await BookingsWithIncludes()
                .Where(b => b.Id == id && b.UserId == userId)
                .Select(ToSummaryDto)
                .FirstOrDefaultAsync();

            if (booking == null) return NotFound();

            return Ok(booking);
        }

        [HttpPost("{id}/share-receipt")]
        public async Task<IActionResult> ShareReceipt(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var owned = await _context.Bookings.AnyAsync(b => b.Id == id && b.UserId == userId);
            if (!owned) return NotFound(new { message = "Booking not found." });

            var result = await ReceiptShareHelper.CreateOrReuseShareTokenAsync(id, _context);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] OnlineBookingDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var user = await _context.EndUsers.FindAsync(userId);
            if (user == null) return Unauthorized();

            var plan = await _context.Plans.FirstOrDefaultAsync(p =>
                p.Id == dto.PlanId && p.LibraryId == dto.LibraryId && !p.IsDeleted);
            if (plan == null || !plan.IsEnabled)
            {
                return BadRequest(new { message = "This plan is no longer available. Please choose another plan." });
            }

            var area = await _context.Areas.FirstOrDefaultAsync(a => a.Id == dto.AreaId && a.LibraryId == dto.LibraryId);
            if (area == null)
            {
                return BadRequest(new { message = "This area could not be found for the selected library." });
            }

            var seat = await _context.Seats.FirstOrDefaultAsync(s => s.Id == dto.SeatId && s.AreaId == dto.AreaId);
            if (seat == null || seat.IsInactive)
            {
                return BadRequest(new { message = "This seat is no longer available." });
            }

            // Price is always derived on the server from the current plan/area/seat data -
            // the client-supplied price is never trusted, so it cannot be tampered with
            // via the network/devtools to pay less than the real price.
            var price = ComputeSeatPrice(area, seat, plan);

            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // A student can only hold one active/ongoing plan per library at a time.
                // "Ongoing" means any non-cancelled booking whose window hasn't fully ended
                // yet (covers Active, Expiring, and still-unpaid bookings that haven't ended) -
                // matches the frontend's "in window" concept in src/lib/status.ts.
                var hasActivePlan = await _context.Bookings.AnyAsync(b =>
                    b.LibraryId == dto.LibraryId &&
                    b.UserId == userId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.EndDate >= DateTime.UtcNow.Date);

                if (hasActivePlan)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "You already have an active plan at this library. Cancel your existing plan or wait for it to end before booking a new one." });
                }

                // Fetch the incoming shift to check for time overlaps
                var incomingShift = await _context.ShiftTemplates.FindAsync(dto.ShiftId);
                if (incomingShift == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Invalid shift selected." });
                }

                // Verify availability - inside the transaction so the check and the insert
                // are atomic and a concurrent booking on the same seat can't slip through.
                // We check if the dates overlap AND the shift times overlap.
                var isConflict = await _context.Bookings
                    .Include(b => b.Shift)
                    .AnyAsync(b =>
                        b.SeatId == dto.SeatId &&
                        b.Status != BookingStatus.Cancelled &&
                        b.StartDate < dto.EndDate &&
                        b.EndDate > dto.StartDate &&
                        b.Shift.StartTime < incomingShift.EndTime &&
                        b.Shift.EndTime > incomingShift.StartTime);

                if (isConflict)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "The selected seat is no longer available for these dates and shift time." });
                }

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    Code = $"BK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    LibraryId = dto.LibraryId,
                    AreaId = dto.AreaId,
                    SeatId = dto.SeatId,
                    PlanId = dto.PlanId,
                    ShiftId = dto.ShiftId,
                    UserId = userId,
                    StudentName = user.Name,
                    StudentContact = user.PhoneNumber,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Status = BookingStatus.Active,
                    Source = BookingSource.Online,
                    PaymentMethod = PaymentMethod.OnlinePrepay,
                    // Payment isn't confirmed yet - the seat is reserved immediately (this whole
                    // block runs inside the Serializable transaction above) but PaymentStatus only
                    // flips to Paid once Cashfree confirms the payment, via the webhook
                    // (CashfreeWebhookController) or the reconciliation fallback
                    // (PaymentsController.GetStatus). See src/components/payment-gateway.tsx for
                    // the matching frontend flow.
                    PaymentStatus = PaymentStatus.Unpaid,
                    PaymentDate = null,
                    Price = price,
                    CreatedAt = DateTime.UtcNow,
                    ConfirmedArrival = true
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _ruleEngine.ProcessBookingCreatedAsync(booking);
                
                // Notify the Owner via Push Notification
                var library = await _context.Libraries.FindAsync(dto.LibraryId);
                if (library != null)
                {
                    var ownerUser = await _context.OwnerUsers.FindAsync(library.OwnerId);
                    if (ownerUser != null && !string.IsNullOrWhiteSpace(ownerUser.FcmToken))
                    {
                        var pushService = HttpContext.RequestServices.GetService(typeof(BMS.API.Modules.Shared.Services.IFirebasePushService)) as BMS.API.Modules.Shared.Services.IFirebasePushService;
                        if (pushService != null)
                        {
                            var title = "New Booking Received!";
                            var body = $"{booking.StudentName} just booked a seat at {library.Name}.";
                            await pushService.SendPushNotificationAsync(ownerUser.FcmToken, title, body);
                        }
                    }
                }

                return Ok(booking);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Mirrors the frontend's computeSeatPrice (src/lib/booking.ts) so the server is the
        /// single source of truth for price: seat override wins outright; otherwise start from
        /// the plan's base price, apply the area's price modifier, then the plan's discounts.
        /// </summary>
        private static decimal ComputeSeatPrice(Area area, Seat seat, Plan plan)
        {
            if (seat.PriceOverride.HasValue) return seat.PriceOverride.Value;

            var price = plan.BasePrice;
            if (area.PriceModifierType.HasValue && area.PriceModifierValue.HasValue)
            {
                if (area.PriceModifierType.Value == PriceModifierType.Flat)
                    price += area.PriceModifierValue.Value;
                else
                    price *= 1 + (area.PriceModifierValue.Value / 100m);
            }
            if (plan.DiscountPercent.HasValue && plan.DiscountPercent.Value > 0)
            {
                price *= 1 - (plan.DiscountPercent.Value / 100m);
            }
            if (plan.DiscountFlat.HasValue && plan.DiscountFlat.Value > 0)
            {
                price = Math.Max(0, price - plan.DiscountFlat.Value);
            }
            return Math.Round(price, 0, MidpointRounding.AwayFromZero);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null || booking.UserId != userId)
                return NotFound();

            if (booking.Status == BookingStatus.Cancelled)
                return BadRequest(new { message = "Booking is already cancelled." });

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking cancelled successfully" });
        }
    }
}
