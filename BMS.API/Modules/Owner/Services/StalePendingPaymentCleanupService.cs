using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BMS.API.Modules.Owner.Services
{
    /// <summary>
    /// A booking created for online prepayment reserves its seat immediately (see
    /// UserBookingsController.CreateBooking), before the student has actually finished paying.
    /// If they abandon checkout (close the tab, payment fails and no webhook ever confirms it,
    /// etc.) that seat would otherwise stay locked forever. This periodically cancels stale
    /// Unpaid online-prepay bookings so the seat becomes bookable again.
    /// </summary>
    public class StalePendingPaymentCleanupService : BackgroundService
    {
        private static readonly TimeSpan PendingPaymentTimeout = TimeSpan.FromMinutes(30);
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StalePendingPaymentCleanupService> _logger;

        public StalePendingPaymentCleanupService(IServiceProvider serviceProvider, ILogger<StalePendingPaymentCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelStaleBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in StalePendingPaymentCleanupService");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CancelStaleBookingsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTime.UtcNow - PendingPaymentTimeout;
            var staleBookings = await context.Bookings
                .Where(b => b.PaymentMethod == PaymentMethod.OnlinePrepay
                            && b.PaymentStatus == PaymentStatus.Unpaid
                            && b.Status == BookingStatus.Active
                            && b.CreatedAt < cutoff)
                .ToListAsync(stoppingToken);

            if (staleBookings.Count == 0) return;

            foreach (var booking in staleBookings)
            {
                booking.Status = BookingStatus.Cancelled;
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Released {Count} stale unpaid booking(s) whose payment was never completed.", staleBookings.Count);
        }
    }
}
