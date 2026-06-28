using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Shared.Models;

namespace BMS.API.Modules.Owner.Services
{
    public interface ILibraryManagementService
    {
        Task<Library> CreateLibraryAsync(Guid ownerId, LibraryCreateDto request);
        Task<Library> UpdateLibraryAsync(Guid libraryId, LibraryUpdateDto request);
        Task<IEnumerable<Library>> GetOwnerLibrariesAsync(Guid ownerId);
        
        Task<Area> AddAreaAsync(Guid libraryId, AreaCreateDto request);
        Task<Seat> AddSeatAsync(Guid areaId, SeatCreateDto request);
        
        Task<ShiftTemplate> AddShiftAsync(Guid libraryId, ShiftTemplate request);
        Task<Plan> AddPlanAsync(Guid libraryId, Plan request);

        Task<Booking> CreateWalkInBookingAsync(WalkInBookingDto request);
        Task<IEnumerable<Booking>> GetLibraryBookingsAsync(Guid libraryId);
    }

    public class LibraryManagementService : ILibraryManagementService
    {
        public Task<Area> AddAreaAsync(Guid libraryId, AreaCreateDto request) => throw new NotImplementedException();
        public Task<Plan> AddPlanAsync(Guid libraryId, Plan request) => throw new NotImplementedException();
        public Task<Seat> AddSeatAsync(Guid areaId, SeatCreateDto request) => throw new NotImplementedException();
        public Task<ShiftTemplate> AddShiftAsync(Guid libraryId, ShiftTemplate request) => throw new NotImplementedException();
        public Task<Library> CreateLibraryAsync(Guid ownerId, LibraryCreateDto request) => throw new NotImplementedException();
        public Task<Booking> CreateWalkInBookingAsync(WalkInBookingDto request) => throw new NotImplementedException();
        public Task<IEnumerable<Booking>> GetLibraryBookingsAsync(Guid libraryId) => throw new NotImplementedException();
        public Task<IEnumerable<Library>> GetOwnerLibrariesAsync(Guid ownerId) => throw new NotImplementedException();
        public Task<Library> UpdateLibraryAsync(Guid libraryId, LibraryUpdateDto request) => throw new NotImplementedException();
    }
}
