using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.User.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BMS.API.Modules.Owner.Services
{
    public class ExpiryNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ExpiryNotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiryNotificationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Log error here in a real application
                    Console.WriteLine($"Error in ExpiryNotificationService: {ex.Message}");
                }

                // Wait for an hour before checking again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessExpiryNotificationsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var targetDate = DateTime.UtcNow.Date.AddDays(7);

            var expiringBookings = await context.Bookings
                .Include(b => b.Library)
                .Where(b => !b.IsDeactivated &&
                            b.Status != BMS.API.Modules.Shared.Models.BookingStatus.Cancelled &&
                            !b.ExpiryNotificationSent &&
                            b.EndDate.Date == targetDate)
                .ToListAsync(stoppingToken);

            foreach (var booking in expiringBookings)
            {
                var rule = await context.OwnerNotificationRules
                    .FirstOrDefaultAsync(r => r.OwnerId == booking.Library.OwnerId && r.RuleType == "expiry", stoppingToken);

                if (rule != null && rule.IsEnabled)
                {
                    var subject = string.IsNullOrEmpty(rule.SubjectTemplate) ? "Your plan expires soon!" : rule.SubjectTemplate;
                    var body = string.IsNullOrEmpty(rule.BodyTemplate) 
                        ? "Hi there, your seat plan at {Library Name} is expiring in less than 7 days. Please renew your plan at the desk to keep your seat. Thanks!" 
                        : rule.BodyTemplate;

                    body = body.Replace("{Library Name}", booking.Library.Name);
                    subject = subject.Replace("{Library Name}", booking.Library.Name);

                    var targetUserId = booking.UserId;
                    if (targetUserId == null && !string.IsNullOrEmpty(booking.StudentContact))
                    {
                        var user = await context.EndUsers.FirstOrDefaultAsync(u => u.PhoneNumber == booking.StudentContact, stoppingToken);
                        if (user != null) targetUserId = user.Id;
                    }

                    if (targetUserId != null)
                    {
                        var notification = new UserNotification
                        {
                            Id = Guid.NewGuid(),
                            UserId = targetUserId.Value,
                            Title = subject,
                            Body = body,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        context.UserNotifications.Add(notification);
                    }
                }

                // Mark as sent regardless so we don't keep evaluating it
                booking.ExpiryNotificationSent = true;
            }

            if (expiringBookings.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
