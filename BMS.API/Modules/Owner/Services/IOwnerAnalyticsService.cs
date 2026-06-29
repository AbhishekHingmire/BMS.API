using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;

namespace BMS.API.Modules.Owner.Services
{
    public interface IOwnerAnalyticsService
    {
        Task<OwnerAnalyticsDto> GetAnalyticsAsync(Guid ownerId, Guid? libraryId = null);
    }
}
