using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;

namespace BMS.API.Modules.Owner.Services
{
    public interface IOwnerProfileService
    {
        Task<OwnerProfileDto> GetProfileAsync(Guid ownerId);
        Task<OwnerProfileDto> UpdateProfileAsync(Guid ownerId, UpdateOwnerProfileDto request);
        
        Task<IEnumerable<NotificationRuleDto>> GetRulesAsync(Guid ownerId);
        Task<NotificationRuleDto> UpdateRuleAsync(Guid ownerId, UpdateNotificationRuleDto request);
        
        Task<BroadcastHistoryDto> CreateBroadcastAsync(Guid ownerId, BroadcastDto request);
        Task<int> GetAudienceCountAsync(Guid ownerId, string libraryId, string audience);
        Task<BroadcastHistoryDto> UpdateBroadcastAsync(Guid ownerId, Guid broadcastId, BroadcastDto request);
        Task<bool> DeleteBroadcastAsync(Guid ownerId, Guid broadcastId);
        Task<IEnumerable<BroadcastHistoryDto>> GetBroadcastHistoryAsync(Guid ownerId);
    }
}
