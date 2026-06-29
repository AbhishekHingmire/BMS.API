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
                SmsNotificationsEnabled = user.SmsNotificationsEnabled
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

            return rules.Select(r => new NotificationRuleDto
            {
                Id = r.Id,
                RuleType = r.RuleType,
                IsEnabled = r.IsEnabled
            });
        }

        public async Task<NotificationRuleDto> UpdateRuleAsync(Guid ownerId, UpdateNotificationRuleDto request)
        {
            var rule = await _context.OwnerNotificationRules
                .FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.RuleType == request.RuleType);

            if (rule == null)
            {
                rule = new OwnerNotificationRule
                {
                    Id = Guid.NewGuid(),
                    OwnerId = ownerId,
                    RuleType = request.RuleType,
                    IsEnabled = request.IsEnabled
                };
                _context.OwnerNotificationRules.Add(rule);
            }
            else
            {
                rule.IsEnabled = request.IsEnabled;
            }

            await _context.SaveChangesAsync();

            return new NotificationRuleDto
            {
                Id = rule.Id,
                RuleType = rule.RuleType,
                IsEnabled = rule.IsEnabled
            };
        }

        public async Task<BroadcastHistoryDto> CreateBroadcastAsync(Guid ownerId, BroadcastDto request)
        {
            // Simplified broadcast creation
            var broadcast = new OwnerBroadcastHistory
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                Subject = request.Subject,
                Message = request.Message,
                Audience = request.Audience,
                LibraryName = request.LibraryName,
                EstimatedRecipients = 0, // Mock, actual system might query bookings
                SentAt = DateTime.UtcNow
            };

            _context.OwnerBroadcastHistories.Add(broadcast);
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
