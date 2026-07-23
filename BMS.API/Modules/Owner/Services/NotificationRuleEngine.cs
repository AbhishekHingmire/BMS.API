using System;
using System.Linq;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using BMS.API.Modules.User.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BMS.API.Modules.Owner.Services
{
    public class NotificationRuleEngine : INotificationRuleEngine
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationRuleEngine(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ProcessBookingCreatedAsync(Booking booking)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var targetUserId = booking.UserId;
            if (targetUserId == null && !string.IsNullOrEmpty(booking.StudentContact))
            {
                var user = await context.EndUsers.FirstOrDefaultAsync(u => u.PhoneNumber == booking.StudentContact);
                if (user != null) targetUserId = user.Id;
            }

            if (targetUserId == null) return; // Only notify if it's tied to an actual user account
            


            var library = await context.Libraries.FindAsync(booking.LibraryId);
            if (library == null) return;

            // Welcome Rule
            var isFirstBooking = !await context.Bookings.AnyAsync(b => b.UserId == targetUserId && b.LibraryId == booking.LibraryId && b.Id != booking.Id);
            if (isFirstBooking)
            {
                await EvaluateAndSendRuleAsync(context, library.OwnerId, targetUserId.Value, library.Name, "welcome", 
                    "Welcome to {Library Name}!", 
                    "We're excited to have you! Please let us know if you need any assistance getting settled into your new seat.");
            }

            // Receipt Rule if Paid on creation
            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                await EvaluateAndSendRuleAsync(context, library.OwnerId, targetUserId.Value, library.Name, "receipts",
                    "Payment Received",
                    "Thank you for your payment. Your plan at {Library Name} is now active. Enjoy your study time!");
            }
        }

        public async Task ProcessBookingPaymentUpdatedAsync(Booking booking)
        {
            if (booking.PaymentStatus != PaymentStatus.Paid) return;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var targetUserId = booking.UserId;
            if (targetUserId == null && !string.IsNullOrEmpty(booking.StudentContact))
            {
                var user = await context.EndUsers.FirstOrDefaultAsync(u => u.PhoneNumber == booking.StudentContact);
                if (user != null) targetUserId = user.Id;
            }

            if (targetUserId == null) return;



            var library = await context.Libraries.FindAsync(booking.LibraryId);
            if (library == null) return;

            await EvaluateAndSendRuleAsync(context, library.OwnerId, targetUserId.Value, library.Name, "receipts",
                    "Payment Received",
                    "Thank you for your payment. Your plan at {Library Name} is now active. Enjoy your study time!");
        }

        private async Task EvaluateAndSendRuleAsync(ApplicationDbContext context, Guid ownerId, Guid userId, string libraryName, string ruleType, string fallbackSubject, string fallbackBody)
        {
            var rule = await context.OwnerNotificationRules.FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.RuleType == ruleType);
            if (rule == null || !rule.IsEnabled) return;

            var subject = string.IsNullOrEmpty(rule.SubjectTemplate) ? fallbackSubject : rule.SubjectTemplate;
            var body = string.IsNullOrEmpty(rule.BodyTemplate) ? fallbackBody : rule.BodyTemplate;

            body = body.Replace("{Library Name}", libraryName);
            subject = subject.Replace("{Library Name}", libraryName);

            var notification = new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = subject,
                Body = body,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.UserNotifications.Add(notification);
            await context.SaveChangesAsync();
            
            // Send Push Notification
            var user = await context.EndUsers.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.FcmToken))
            {
                using var scope = _serviceProvider.CreateScope();
                var pushService = scope.ServiceProvider.GetRequiredService<BMS.API.Modules.Shared.Services.IFirebasePushService>();
                await pushService.SendPushNotificationAsync(user.FcmToken, subject, body);
            }
        }
    }
}
