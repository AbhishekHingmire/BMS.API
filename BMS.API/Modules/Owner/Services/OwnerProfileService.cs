using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Models;
using BMS.API.Modules.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Owner.Services
{
    public class OwnerProfileService : IOwnerProfileService
    {
        private readonly ApplicationDbContext _context;

        public OwnerProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OwnerProfileDto> GetProfileAsync(Guid ownerId)
        {
            var user = await _context.OwnerUsers.FindAsync(ownerId);
            if (user == null) return null;

            return new OwnerProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                BankName = user.BankName,
                AccountNumber = user.AccountNumber,
                IfscCode = user.IfscCode,
                UpiId = user.UpiId,
                EmailNotificationsEnabled = user.EmailNotificationsEnabled,
                SmsNotificationsEnabled = user.SmsNotificationsEnabled,
                Plan = user.Plan,
                PlanStartedAt = user.PlanStartedAt
            };
        }

        public async Task<OwnerProfileDto> UpdateProfileAsync(Guid ownerId, UpdateOwnerProfileDto request)
        {
            var user = await _context.OwnerUsers.FindAsync(ownerId);
            if (user == null) return null;

            user.Name = request.Name;
            user.PhoneNumber = request.PhoneNumber;
            user.AvatarUrl = request.AvatarUrl;
            user.BankName = request.BankName;
            user.AccountNumber = request.AccountNumber;
            user.IfscCode = request.IfscCode;
            user.UpiId = request.UpiId;
            user.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
            user.SmsNotificationsEnabled = request.SmsNotificationsEnabled;

            await _context.SaveChangesAsync();

            return await GetProfileAsync(ownerId);
        }

        public async Task<IEnumerable<NotificationRuleDto>> GetRulesAsync(Guid ownerId)
        {
            var rules = await _context.OwnerNotificationRules
                .Where(r => r.OwnerId == ownerId)
                .ToListAsync();

            var defaultRules = new List<NotificationRuleDto>
            {
                new NotificationRuleDto { RuleType = "expiry", SubjectTemplate = "Your plan expires soon!", BodyTemplate = "Hi there, your seat plan at {Library Name} is expiring in less than 7 days. Please renew your plan at the desk to keep your seat. Thanks!" },
                new NotificationRuleDto { RuleType = "receipts", SubjectTemplate = "Payment Received", BodyTemplate = "Thank you for your payment. Your plan at {Library Name} is now active. Enjoy your study time!" },
                new NotificationRuleDto { RuleType = "welcome", SubjectTemplate = "Welcome to {Library Name}!", BodyTemplate = "We're excited to have you! Please let us know if you need any assistance getting settled into your new seat." },
                new NotificationRuleDto { RuleType = "offer", SubjectTemplate = "Special Offer on Renewals!", BodyTemplate = "Renew your plan at {Library Name} this week and get an extra 3 days added for free. Visit the desk to claim this offer." }
            };

            var result = new List<NotificationRuleDto>();
            foreach (var def in defaultRules)
            {
                var existing = rules.FirstOrDefault(r => r.RuleType == def.RuleType);
                if (existing != null)
                {
                    result.Add(new NotificationRuleDto
                    {
                        Id = existing.Id,
                        RuleType = existing.RuleType,
                        IsEnabled = existing.IsEnabled,
                        SubjectTemplate = string.IsNullOrEmpty(existing.SubjectTemplate) ? def.SubjectTemplate : existing.SubjectTemplate,
                        BodyTemplate = string.IsNullOrEmpty(existing.BodyTemplate) ? def.BodyTemplate : existing.BodyTemplate
                    });
                }
                else
                {
                    result.Add(new NotificationRuleDto
                    {
                        Id = Guid.Empty,
                        RuleType = def.RuleType,
                        IsEnabled = false,
                        SubjectTemplate = def.SubjectTemplate,
                        BodyTemplate = def.BodyTemplate
                    });
                }
            }

            return result;
        }

        public async Task<NotificationRuleDto> UpdateRuleAsync(Guid ownerId, UpdateNotificationRuleDto request)
        {
            var rule = await _context.OwnerNotificationRules
                .FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.RuleType == request.RuleType);

            if (rule == null)
            {
                var ownerExists = await _context.OwnerUsers.AnyAsync(u => u.Id == ownerId);
                if (!ownerExists)
                {
                    throw new UnauthorizedAccessException("Owner account no longer exists. Please log in again.");
                }
                rule = new OwnerNotificationRule
                {
                    Id = Guid.NewGuid(),
                    OwnerId = ownerId,
                    RuleType = request.RuleType,
                    IsEnabled = request.IsEnabled,
                    SubjectTemplate = request.SubjectTemplate ?? "",
                    BodyTemplate = request.BodyTemplate ?? ""
                };
                _context.OwnerNotificationRules.Add(rule);
            }
            else
            {
                rule.IsEnabled = request.IsEnabled;
                if (request.SubjectTemplate != null) rule.SubjectTemplate = request.SubjectTemplate;
                if (request.BodyTemplate != null) rule.BodyTemplate = request.BodyTemplate;
            }

            await _context.SaveChangesAsync();

            return new NotificationRuleDto
            {
                Id = rule.Id,
                RuleType = rule.RuleType,
                IsEnabled = rule.IsEnabled,
                SubjectTemplate = rule.SubjectTemplate,
                BodyTemplate = rule.BodyTemplate
            };
        }

        public async Task<BroadcastHistoryDto> CreateBroadcastAsync(Guid ownerId, BroadcastDto request)
        {
            var ownerExists = await _context.OwnerUsers.AnyAsync(u => u.Id == ownerId);
            if (!ownerExists)
            {
                throw new UnauthorizedAccessException("Owner account no longer exists. Please log in again.");
            }

            var broadcast = new OwnerBroadcastHistory
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                Subject = request.Subject,
                Message = request.Message,
                Audience = request.Audience,
                LibraryName = request.LibraryName,
                SentAt = DateTime.UtcNow
            };

            _context.OwnerBroadcastHistories.Add(broadcast);

            var targetUserIds = await GetTargetUserIdsAsync(ownerId, request.LibraryId, request.Audience);

            broadcast.EstimatedRecipients = targetUserIds.Count;

            var notifications = targetUserIds.Select(userId => new BMS.API.Modules.User.Models.UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Subject,
                Body = request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                BroadcastId = broadcast.Id
            }).ToList();

            _context.UserNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return new BroadcastHistoryDto
            {
                Id = broadcast.Id,
                Subject = broadcast.Subject,
                Message = broadcast.Message,
                Audience = broadcast.Audience,
                LibraryName = broadcast.LibraryName,
                EstimatedRecipients = broadcast.EstimatedRecipients,
                SentAt = broadcast.SentAt
            };
        }

        public async Task<int> GetAudienceCountAsync(Guid ownerId, string libraryId, string audience)
        {
            var targetBookings = await GetTargetBookingsAsync(ownerId, libraryId, audience);
            return targetBookings.Count;
        }

        private async Task<List<BMS.API.Modules.Shared.Models.Booking>> GetTargetBookingsAsync(Guid ownerId, string libraryId, string audience)
        {
            var today = DateTime.UtcNow.Date;
            
            var bookingsQuery = _context.Bookings
                .Include(b => b.Library)
                .Where(b => b.Library.OwnerId == ownerId && !b.IsDeactivated && b.Status != BMS.API.Modules.Shared.Models.BookingStatus.Cancelled);

            if (!string.IsNullOrEmpty(libraryId) && libraryId != "all" && Guid.TryParse(libraryId, out Guid parsedLibId))
            {
                bookingsQuery = bookingsQuery.Where(b => b.LibraryId == parsedLibId);
            }

            var allBookings = await bookingsQuery.ToListAsync();

            var latestBookings = allBookings
                .GroupBy(b => b.UserId?.ToString() ?? (!string.IsNullOrEmpty(b.StudentContact) ? b.StudentContact : (b.StudentName ?? "unknown")))
                .Select(g => g.OrderByDescending(x => x.EndDate).First())
                .ToList();

            if (audience == "active")
            {
                latestBookings = latestBookings.Where(b => b.Status == BMS.API.Modules.Shared.Models.BookingStatus.Active).ToList();
            }
            else if (audience == "expiring")
            {
                latestBookings = latestBookings.Where(b => (b.Status == BMS.API.Modules.Shared.Models.BookingStatus.Active) && (b.EndDate.Date - today).TotalDays >= 0 && (b.EndDate.Date - today).TotalDays <= 7).ToList();
            }
            else if (audience == "expired")
            {
                latestBookings = latestBookings.Where(b => b.Status == BMS.API.Modules.Shared.Models.BookingStatus.Expired).ToList();
            }

            return latestBookings;
        }

        private async Task<List<Guid>> GetTargetUserIdsAsync(Guid ownerId, string libraryId, string audience)
        {
            var latestBookings = await GetTargetBookingsAsync(ownerId, libraryId, audience);

            var targetContacts = latestBookings
                .Where(b => !string.IsNullOrEmpty(b.StudentContact))
                .Select(b => b.StudentContact)
                .Distinct()
                .ToList();

            var targetUserIdsByContact = await _context.EndUsers
                .Where(u => targetContacts.Contains(u.PhoneNumber))
                .Select(u => u.Id)
                .ToListAsync();

            var targetUserIdsByBooking = latestBookings
                .Where(b => b.UserId != null)
                .Select(b => b.UserId.Value)
                .ToList();

            return targetUserIdsByContact.Union(targetUserIdsByBooking).Distinct().ToList();
        }

        public async Task<BroadcastHistoryDto> UpdateBroadcastAsync(Guid ownerId, Guid broadcastId, BroadcastDto request)
        {
            var broadcast = await _context.OwnerBroadcastHistories
                .FirstOrDefaultAsync(b => b.Id == broadcastId && b.OwnerId == ownerId);
                
            if (broadcast == null) return null;

            broadcast.Subject = request.Subject;
            broadcast.Message = request.Message;

            var notifications = await _context.UserNotifications
                .Where(n => n.BroadcastId == broadcastId)
                .ToListAsync();

            foreach(var notification in notifications)
            {
                notification.Title = request.Subject;
                notification.Body = request.Message;
            }

            await _context.SaveChangesAsync();

            return new BroadcastHistoryDto
            {
                Id = broadcast.Id,
                Subject = broadcast.Subject,
                Message = broadcast.Message,
                Audience = broadcast.Audience,
                LibraryName = broadcast.LibraryName,
                EstimatedRecipients = broadcast.EstimatedRecipients,
                SentAt = broadcast.SentAt
            };
        }

        public async Task<bool> DeleteBroadcastAsync(Guid ownerId, Guid broadcastId)
        {
            var broadcast = await _context.OwnerBroadcastHistories
                .FirstOrDefaultAsync(b => b.Id == broadcastId && b.OwnerId == ownerId);
                
            if (broadcast == null) return false;

            _context.OwnerBroadcastHistories.Remove(broadcast);
            
            var notifications = await _context.UserNotifications
                .Where(n => n.BroadcastId == broadcastId)
                .ToListAsync();
                
            _context.UserNotifications.RemoveRange(notifications);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BroadcastHistoryDto>> GetBroadcastHistoryAsync(Guid ownerId)
        {
            var history = await _context.OwnerBroadcastHistories
                .Where(b => b.OwnerId == ownerId)
                .OrderByDescending(b => b.SentAt)
                .ToListAsync();

            return history.Select(h => new BroadcastHistoryDto
            {
                Id = h.Id,
                Subject = h.Subject,
                Message = h.Message,
                Audience = h.Audience,
                LibraryName = h.LibraryName,
                EstimatedRecipients = h.EstimatedRecipients,
                SentAt = h.SentAt
            });
        }
    }
}
