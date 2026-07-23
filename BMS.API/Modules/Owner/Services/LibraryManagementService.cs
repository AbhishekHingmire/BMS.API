using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Owner.Services
{
    public interface ILibraryManagementService
    {
        Task<LibraryResponseDto> CreateLibraryAsync(Guid ownerId, LibraryCreateDto request);
        Task<LibraryResponseDto> UpdateLibraryAsync(Guid libraryId, LibraryUpdateDto request);
        Task<IEnumerable<LibraryResponseDto>> GetOwnerLibrariesAsync(Guid ownerId);
        Task<LibraryResponseDto> GetLibraryByIdAsync(Guid libraryId);
        Task<bool> DeleteLibraryAsync(Guid libraryId);
        
        Task<Area> AddAreaAsync(Guid libraryId, AreaCreateDto request);
        Task<Area> UpdateAreaAsync(Guid areaId, AreaCreateDto request);
        Task<bool> DeleteAreaAsync(Guid areaId);
        Task<IEnumerable<Area>> GetAreasAsync(Guid libraryId);

        Task<Seat> AddSeatAsync(Guid areaId, SeatCreateDto request);
        Task<Seat> UpdateSeatAsync(Guid seatId, SeatCreateDto request);
        Task<Seat> ToggleSeatRestrictionAsync(Guid seatId);
        Task<bool> DeleteSeatAsync(Guid seatId);
        Task<IEnumerable<Seat>> GetSeatsAsync(Guid areaId);
        
        Task<ShiftTemplate> AddShiftAsync(Guid libraryId, ShiftTemplate request);
        Task<ShiftTemplate> UpdateShiftAsync(Guid shiftId, ShiftTemplate request);
        Task<bool> DeleteShiftAsync(Guid shiftId);
        Task<IEnumerable<ShiftTemplate>> GetShiftsAsync(Guid libraryId);

        Task<Plan> AddPlanAsync(Guid libraryId, Plan request);
        Task<Plan> UpdatePlanAsync(Guid planId, Plan request);
        Task<bool> DeletePlanAsync(Guid planId);
        Task<IEnumerable<Plan>> GetPlansAsync(Guid libraryId);

        Task<Booking> CreateWalkInBookingAsync(WalkInBookingDto request);
        Task<Booking> UpdateBookingAsync(Guid bookingId, UpdateBookingDto request);
        Task<IEnumerable<Booking>> GetLibraryBookingsAsync(Guid libraryId);
        Task<Booking> GetBookingByCodeAsync(Guid ownerId, string code);

        // Ownership / authorization helpers - used by controllers to enforce that an
        // authenticated owner can only read/mutate resources under libraries they own.
        Task<bool> IsLibraryOwnedByAsync(Guid libraryId, Guid ownerId);
        Task<bool> IsAreaOwnedByAsync(Guid areaId, Guid ownerId);
        Task<bool> IsSeatOwnedByAsync(Guid seatId, Guid ownerId);
        Task<bool> IsPlanOwnedByAsync(Guid planId, Guid ownerId);
        Task<bool> IsShiftOwnedByAsync(Guid shiftId, Guid ownerId);
        Task<bool> IsBookingOwnedByAsync(Guid bookingId, Guid ownerId);
        Task<int> CountActiveBookingsAsync(Guid libraryId);
    }

    public class LibraryManagementService : ILibraryManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationRuleEngine _ruleEngine;

        public LibraryManagementService(ApplicationDbContext context, INotificationRuleEngine ruleEngine)
        {
            _context = context;
            _ruleEngine = ruleEngine;
        }

        private LibraryResponseDto MapToLibraryResponseDto(Library l)
        {
            if (l == null) return null;
            return new LibraryResponseDto
            {
                Id = l.Id,
                OwnerId = l.OwnerId,
                Name = l.Name,
                Description = l.Description,
                Address = l.Address,
                Area = l.AreaName,
                City = l.City,
                Photos = l.Photos ?? new List<string>(),
                Amenities = string.IsNullOrEmpty(l.AmenitiesString) ? new List<string>() : l.AmenitiesString.Split(',').ToList(),
                Verified = l.IsVerified,
                Published = l.IsPublished,
                CancellationPolicy = l.CancellationPolicy,
                FaqJson = string.IsNullOrEmpty(l.FaqJson) ? "[]" : l.FaqJson,
                Shifts = l.Shifts.Where(s => !s.IsDeleted).Select(s => new ShiftResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Start = s.StartTime.ToString(@"hh\:mm"),
                    End = s.EndTime.ToString(@"hh\:mm")
                }).ToList(),
                Areas = l.Areas.Where(a => !a.IsDeleted).Select(a => new AreaResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Tags = string.IsNullOrEmpty(a.TagsString) ? new List<string>() : a.TagsString.Split(',').ToList(),
                    PriceModifier = a.PriceModifierType.HasValue ? new { type = a.PriceModifierType.Value.ToString().ToLower(), value = a.PriceModifierValue } : null,
                    PlanOverrideIds = string.IsNullOrEmpty(a.PlanOverrideIdsJson) ? new List<Guid>() : (System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(a.PlanOverrideIdsJson) ?? new List<Guid>()),
                    FloorPlan = string.IsNullOrEmpty(a.FloorPlanJson) ? null : System.Text.Json.JsonSerializer.Deserialize<object>(a.FloorPlanJson),
                    Seats = a.Seats.Where(st => !st.IsDeleted).Select(st => new SeatResponseDto
                    {
                        Id = st.Id,
                        Number = st.Number,
                        GenderRestriction = st.GenderRestriction == GenderRestriction.None ? null : st.GenderRestriction.ToString().ToLower(),
                        PriceOverride = st.PriceOverride,
                        Inactive = st.IsInactive
                    }).ToList()
                }).ToList(),
                Plans = l.Plans.Select(p => new PlanResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Duration = p.Duration.ToString().ToLower(),
                    ShiftId = p.ShiftId == Guid.Empty ? null : p.ShiftId,
                    BasePrice = p.BasePrice,
                    DiscountPercent = p.DiscountPercent,
                    DiscountFlat = p.DiscountFlat,
                    Enabled = p.IsEnabled,
                    DaysOfWeek = string.IsNullOrEmpty(p.DaysOfWeekString) ? new List<int>() : p.DaysOfWeekString.Split(',').Select(int.Parse).ToList()
                }).ToList()
            };
        }

        public async Task<LibraryResponseDto> CreateLibraryAsync(Guid ownerId, LibraryCreateDto request)
        {
            var library = new Library
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                Name = request.Name ?? "",
                Description = request.Description ?? "",
                Address = request.Address ?? "",
                AreaName = request.AreaName ?? "",
                City = request.City ?? "",
                AmenitiesString = request.AmenitiesString ?? "",
                PhotosJson = request.Photos != null ? System.Text.Json.JsonSerializer.Serialize(request.Photos) : "[]",
                CancellationPolicy = request.CancellationPolicy ?? "",
                FaqJson = string.IsNullOrEmpty(request.FaqJson) ? "[]" : request.FaqJson,
                IsVerified = true,
                IsPublished = request.IsPublished
            };

            _context.Libraries.Add(library);

            var shiftIdMapping = new Dictionary<Guid, Guid>();

            // Process nested Shifts
            foreach (var sDto in request.Shifts)
            {
                var newShiftId = Guid.NewGuid();
                if (sDto.Id.HasValue && sDto.Id.Value != Guid.Empty)
                {
                    shiftIdMapping[sDto.Id.Value] = newShiftId;
                }

                var shift = new ShiftTemplate
                {
                    Id = newShiftId,
                    LibraryId = library.Id,
                    Name = sDto.Name ?? "Untitled Shift",
                    StartTime = sDto.StartTime,
                    EndTime = sDto.EndTime
                };
                _context.ShiftTemplates.Add(shift);
            }

            // Process nested Areas and Seats
            foreach (var aDto in request.Areas)
            {
                var newAreaId = Guid.NewGuid();
                var area = new Area
                {
                    Id = newAreaId,
                    LibraryId = library.Id,
                    Name = aDto.Name ?? "Untitled Area",
                    TagsString = aDto.TagsString,
                    PriceModifierType = aDto.PriceModifierType,
                    PriceModifierValue = aDto.PriceModifierValue,
                    FloorPlanJson = aDto.FloorPlanJson ?? "",
                    PlanOverrideIdsJson = aDto.PlanOverrideIdsJson ?? ""
                };
                _context.Areas.Add(area);

                foreach (var seatDto in aDto.Seats)
                {
                    var seat = new Seat
                    {
                        Id = Guid.NewGuid(),
                        AreaId = newAreaId,
                        Number = seatDto.Number ?? "U1",
                        GenderRestriction = seatDto.GenderRestriction,
                        PriceOverride = seatDto.PriceOverride
                    };
                    _context.Seats.Add(seat);
                }
            }

            // Process nested Plans
            foreach (var pDto in request.Plans)
            {
                var mappedShiftId = Guid.Empty;
                if (pDto.ShiftId.HasValue)
                {
                    if (shiftIdMapping.TryGetValue(pDto.ShiftId.Value, out var newId))
                    {
                        mappedShiftId = newId;
                    }
                    else
                    {
                        mappedShiftId = pDto.ShiftId.Value;
                    }
                }

                var plan = new Plan
                {
                    Id = Guid.NewGuid(),
                    LibraryId = library.Id,
                    Name = pDto.Name ?? "Untitled Plan",
                    Duration = pDto.Duration,
                    ShiftId = mappedShiftId,
                    BasePrice = pDto.BasePrice,
                    DiscountPercent = pDto.DiscountPercent,
                    DiscountFlat = pDto.DiscountFlat,
                    IsEnabled = pDto.IsEnabled,
                    DaysOfWeekString = pDto.DaysOfWeekString ?? "1,2,3,4,5,6,7"
                };
                _context.Plans.Add(plan);
            }

            await _context.SaveChangesAsync();
            
            // Reload with relations to map correctly
            library = await _context.Libraries
                .Include(l => l.Areas).ThenInclude(a => a.Seats)
                .Include(l => l.Shifts)
                .Include(l => l.Plans)
                .FirstOrDefaultAsync(l => l.Id == library.Id);
                
            return MapToLibraryResponseDto(library);
        }

        public async Task<LibraryResponseDto> UpdateLibraryAsync(Guid libraryId, LibraryUpdateDto request)
        {
            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) return null;

            library.Name = request.Name;
            library.Description = request.Description;
            library.Address = request.Address;
            library.AreaName = request.AreaName;
            library.City = request.City;
            library.AmenitiesString = request.AmenitiesString;
            library.PhotosJson = request.Photos != null ? System.Text.Json.JsonSerializer.Serialize(request.Photos) : "[]";
            library.CancellationPolicy = request.CancellationPolicy;
            library.FaqJson = string.IsNullOrEmpty(request.FaqJson) ? "[]" : request.FaqJson;
            library.IsPublished = request.IsPublished;

            // Sync Shifts. Shifts can be referenced by Bookings (FK on Bookings.ShiftId), so a
            // shift that has ever been used can't be hard-deleted - it is soft-deleted instead
            // and excluded from every read path.
            var existingShifts = await _context.ShiftTemplates.Where(s => s.LibraryId == libraryId && !s.IsDeleted).ToListAsync();
            foreach (var existingShift in existingShifts)
            {
                if (!request.Shifts.Any(s => s.Id == existingShift.Id))
                {
                    existingShift.IsDeleted = true;
                }
            }
            var shiftIdMapping = new Dictionary<Guid, Guid>();
            foreach (var reqShift in request.Shifts)
            {
                var existing = existingShifts.FirstOrDefault(s => s.Id == reqShift.Id);
                if (existing != null)
                {
                    existing.Name = reqShift.Name;
                    existing.StartTime = reqShift.StartTime;
                    existing.EndTime = reqShift.EndTime;
                    shiftIdMapping[existing.Id] = existing.Id;
                }
                else
                {
                    var newShiftId = Guid.NewGuid();
                    if (reqShift.Id.HasValue && reqShift.Id.Value != Guid.Empty)
                    {
                        shiftIdMapping[reqShift.Id.Value] = newShiftId;
                    }
                    _context.ShiftTemplates.Add(new ShiftTemplate
                    {
                        Id = newShiftId,
                        LibraryId = libraryId,
                        Name = reqShift.Name,
                        StartTime = reqShift.StartTime,
                        EndTime = reqShift.EndTime
                    });
                }
            }

            // Sync Areas and Seats. Seats (and their parent Areas) can be referenced by
            // Bookings (FK on Bookings.SeatId/AreaId) even after the booking is cancelled or
            // completed, so a hard delete would throw a DbUpdateException the moment the area
            // or seat has ever been booked. Soft-delete instead so booking history stays intact
            // and the row is simply excluded from every read path (owner UI, browse, booking).
            var existingAreas = await _context.Areas
                .Include(a => a.Seats.Where(s => !s.IsDeleted))
                .Where(a => a.LibraryId == libraryId && !a.IsDeleted)
                .ToListAsync();
            foreach (var existingArea in existingAreas)
            {
                if (!request.Areas.Any(a => a.Id == existingArea.Id))
                {
                    existingArea.IsDeleted = true;
                    foreach (var seat in existingArea.Seats)
                    {
                        seat.IsDeleted = true;
                    }
                }
            }
            foreach (var reqArea in request.Areas)
            {
                var existing = existingAreas.FirstOrDefault(a => a.Id == reqArea.Id);
                Guid currentAreaId;
                if (existing != null)
                {
                    currentAreaId = existing.Id;
                    existing.Name = reqArea.Name;
                    existing.TagsString = reqArea.TagsString;
                    existing.PriceModifierType = reqArea.PriceModifierType;
                    existing.PriceModifierValue = reqArea.PriceModifierValue;
                    existing.FloorPlanJson = reqArea.FloorPlanJson ?? "";
                    existing.PlanOverrideIdsJson = reqArea.PlanOverrideIdsJson ?? "";
                    
                    // Sync Seats (soft-delete - see note above on Areas)
                    foreach (var existingSeat in existing.Seats.ToList())
                    {
                        if (!reqArea.Seats.Any(s => s.Id == existingSeat.Id))
                        {
                            existingSeat.IsDeleted = true;
                        }
                    }
                    foreach (var reqSeat in reqArea.Seats)
                    {
                        var existSeat = existing.Seats.FirstOrDefault(s => s.Id == reqSeat.Id);
                        if (existSeat != null)
                        {
                            existSeat.Number = reqSeat.Number;
                            existSeat.GenderRestriction = reqSeat.GenderRestriction;
                            existSeat.PriceOverride = reqSeat.PriceOverride;
                        }
                        else
                        {
                            _context.Seats.Add(new Seat
                            {
                                Id = Guid.NewGuid(),
                                AreaId = currentAreaId,
                                Number = reqSeat.Number,
                                GenderRestriction = reqSeat.GenderRestriction,
                                PriceOverride = reqSeat.PriceOverride
                            });
                        }
                    }
                }
                else
                {
                    currentAreaId = Guid.NewGuid();
                    var newArea = new Area
                    {
                        Id = currentAreaId,
                        LibraryId = libraryId,
                        Name = reqArea.Name,
                        TagsString = reqArea.TagsString,
                        PriceModifierType = reqArea.PriceModifierType,
                        PriceModifierValue = reqArea.PriceModifierValue,
                        FloorPlanJson = reqArea.FloorPlanJson ?? "",
                        PlanOverrideIdsJson = reqArea.PlanOverrideIdsJson ?? ""
                    };
                    _context.Areas.Add(newArea);
                    foreach (var reqSeat in reqArea.Seats)
                    {
                        _context.Seats.Add(new Seat
                        {
                            Id = Guid.NewGuid(),
                            AreaId = currentAreaId,
                            Number = reqSeat.Number,
                            GenderRestriction = reqSeat.GenderRestriction,
                            PriceOverride = reqSeat.PriceOverride
                        });
                    }
                }
            }

            // Sync Plans
            var existingPlans = await _context.Plans.Where(p => p.LibraryId == libraryId).ToListAsync();
            foreach (var existingPlan in existingPlans)
            {
                if (!request.Plans.Any(p => p.Id == existingPlan.Id))
                {
                    existingPlan.IsDeleted = true;
                }
            }
            foreach (var reqPlan in request.Plans)
            {
                var mappedShiftId = Guid.Empty;
                if (reqPlan.ShiftId.HasValue)
                {
                    if (shiftIdMapping.TryGetValue(reqPlan.ShiftId.Value, out var newId))
                        mappedShiftId = newId;
                    else
                        mappedShiftId = reqPlan.ShiftId.Value;
                }

                var existing = existingPlans.FirstOrDefault(p => p.Id == reqPlan.Id);
                if (existing != null)
                {
                    existing.Name = reqPlan.Name;
                    existing.Duration = reqPlan.Duration;
                    existing.ShiftId = mappedShiftId;
                    existing.BasePrice = reqPlan.BasePrice;
                    existing.DiscountPercent = reqPlan.DiscountPercent;
                    existing.DiscountFlat = reqPlan.DiscountFlat;
                    existing.IsEnabled = reqPlan.IsEnabled;
                    existing.DaysOfWeekString = reqPlan.DaysOfWeekString ?? "1,2,3,4,5,6,7";
                }
                else
                {
                    _context.Plans.Add(new Plan
                    {
                        Id = Guid.NewGuid(),
                        LibraryId = libraryId,
                        Name = reqPlan.Name,
                        Duration = reqPlan.Duration,
                        ShiftId = mappedShiftId,
                        BasePrice = reqPlan.BasePrice,
                        DiscountPercent = reqPlan.DiscountPercent,
                        DiscountFlat = reqPlan.DiscountFlat,
                        IsEnabled = reqPlan.IsEnabled,
                        DaysOfWeekString = reqPlan.DaysOfWeekString ?? "1,2,3,4,5,6,7"
                    });
                }
            }

            await _context.SaveChangesAsync();
            
            var updatedLib = await _context.Libraries
                .Include(l => l.Areas.Where(a => !a.IsDeleted)).ThenInclude(a => a.Seats.Where(s => !s.IsDeleted))
                .Include(l => l.Shifts.Where(s => !s.IsDeleted))
                .Include(l => l.Plans.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(l => l.Id == library.Id);

            // Guard-rail: if this update removed the last seat, shift, or enabled plan, the
            // library can no longer be booked - force it back to unpublished and surface why,
            // rather than silently leaving a "live" listing students can't actually book.
            string autoUnpublishedReason = null;
            if (updatedLib.IsPublished)
            {
                var totalBookableSeats = updatedLib.Areas.Sum(a => a.Seats.Count(s => !s.IsInactive));
                var hasShift = updatedLib.Shifts.Any();
                var hasEnabledPlan = updatedLib.Plans.Any(p => p.IsEnabled);

                if (totalBookableSeats == 0 || !hasShift || !hasEnabledPlan)
                {
                    var missing = new List<string>();
                    if (totalBookableSeats == 0) missing.Add("no bookable seats");
                    if (!hasShift) missing.Add("no shifts");
                    if (!hasEnabledPlan) missing.Add("no enabled plans");

                    updatedLib.IsPublished = false;
                    autoUnpublishedReason = $"This library was automatically unpublished because it now has {string.Join(" and ", missing)}, so students can't book it. Add them back and re-publish when ready.";
                    await _context.SaveChangesAsync();
                }
            }

            var dto = MapToLibraryResponseDto(updatedLib);
            dto.AutoUnpublishedReason = autoUnpublishedReason;
            return dto;
        }

        public async Task<bool> DeleteLibraryAsync(Guid libraryId)
        {
            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) return false;

            _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<LibraryResponseDto>> GetOwnerLibrariesAsync(Guid ownerId)
        {
            var libs = await _context.Libraries
                .Where(l => l.OwnerId == ownerId)
                .Include(l => l.Areas.Where(a => !a.IsDeleted)).ThenInclude(a => a.Seats.Where(s => !s.IsDeleted))
                .Include(l => l.Shifts.Where(s => !s.IsDeleted))
                .Include(l => l.Plans.Where(p => !p.IsDeleted))
                .ToListAsync();
            return libs.Select(MapToLibraryResponseDto);
        }

        public async Task<LibraryResponseDto> GetLibraryByIdAsync(Guid libraryId)
        {
            var lib = await _context.Libraries
                .Include(l => l.Areas.Where(a => !a.IsDeleted)).ThenInclude(a => a.Seats.Where(s => !s.IsDeleted))
                .Include(l => l.Shifts.Where(s => !s.IsDeleted))
                .Include(l => l.Plans.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(l => l.Id == libraryId);
            return MapToLibraryResponseDto(lib);
        }

        public async Task<Area> AddAreaAsync(Guid libraryId, AreaCreateDto request)
        {
            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) throw new ArgumentException("Library not found");

            var area = new Area
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                Name = request.Name,
                TagsString = request.TagsString,
                PriceModifierType = request.PriceModifierType,
                PriceModifierValue = request.PriceModifierValue,
                FloorPlanJson = request.FloorPlanJson,
                PlanOverrideIdsJson = request.PlanOverrideIdsJson
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return area;
        }

        public async Task<Area> UpdateAreaAsync(Guid areaId, AreaCreateDto request)
        {
            var area = await _context.Areas.FindAsync(areaId);
            if (area == null) return null;

            area.Name = request.Name;
            area.TagsString = request.TagsString;
            area.PriceModifierType = request.PriceModifierType;
            area.PriceModifierValue = request.PriceModifierValue;
            area.FloorPlanJson = request.FloorPlanJson;
            area.PlanOverrideIdsJson = request.PlanOverrideIdsJson;

            await _context.SaveChangesAsync();
            return area;
        }

        public async Task<bool> DeleteAreaAsync(Guid areaId)
        {
            var area = await _context.Areas.Include(a => a.Seats).FirstOrDefaultAsync(a => a.Id == areaId);
            if (area == null) return false;

            // Soft-delete (see note in UpdateLibraryAsync) - a hard delete throws a
            // DbUpdateException if any seat in this area has ever been booked.
            area.IsDeleted = true;
            foreach (var seat in area.Seats)
            {
                seat.IsDeleted = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Area>> GetAreasAsync(Guid libraryId)
        {
            return await _context.Areas
                .Where(a => a.LibraryId == libraryId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Seat> AddSeatAsync(Guid areaId, SeatCreateDto request)
        {
            var area = await _context.Areas.FindAsync(areaId);
            if (area == null) throw new ArgumentException("Area not found");

            var seat = new Seat
            {
                Id = Guid.NewGuid(),
                AreaId = areaId,
                Number = request.Number,
                GenderRestriction = request.GenderRestriction,
                PriceOverride = request.PriceOverride,
                IsInactive = false
            };

            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();
            return seat;
        }

        public async Task<Seat> UpdateSeatAsync(Guid seatId, SeatCreateDto request)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null) return null;

            seat.Number = request.Number;
            seat.GenderRestriction = request.GenderRestriction;
            seat.PriceOverride = request.PriceOverride;

            await _context.SaveChangesAsync();
            return seat;
        }

        public async Task<Seat> ToggleSeatRestrictionAsync(Guid seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null) return null;

            seat.IsInactive = !seat.IsInactive;
            await _context.SaveChangesAsync();
            return seat;
        }

        public async Task<bool> DeleteSeatAsync(Guid seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null) return false;

            // Soft-delete (see note in UpdateLibraryAsync) - a hard delete throws a
            // DbUpdateException if this seat has ever been booked.
            seat.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Seat>> GetSeatsAsync(Guid areaId)
        {
            return await _context.Seats
                .Where(s => s.AreaId == areaId && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<ShiftTemplate> AddShiftAsync(Guid libraryId, ShiftTemplate request)
        {
            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) throw new ArgumentException("Library not found");

            request.Id = Guid.NewGuid();
            request.LibraryId = libraryId;
            
            _context.ShiftTemplates.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<ShiftTemplate> UpdateShiftAsync(Guid shiftId, ShiftTemplate request)
        {
            var shift = await _context.ShiftTemplates.FindAsync(shiftId);
            if (shift == null) return null;

            shift.Name = request.Name;
            shift.StartTime = request.StartTime;
            shift.EndTime = request.EndTime;

            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<bool> DeleteShiftAsync(Guid shiftId)
        {
            var shift = await _context.ShiftTemplates.FindAsync(shiftId);
            if (shift == null) return false;

            // Soft-delete (see note in UpdateLibraryAsync) - a hard delete throws a
            // DbUpdateException if this shift has ever been booked.
            shift.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ShiftTemplate>> GetShiftsAsync(Guid libraryId)
        {
            return await _context.ShiftTemplates
                .Where(s => s.LibraryId == libraryId && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<Plan> AddPlanAsync(Guid libraryId, Plan request)
        {
            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) throw new ArgumentException("Library not found");

            request.Id = Guid.NewGuid();
            request.LibraryId = libraryId;

            _context.Plans.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<Plan> UpdatePlanAsync(Guid planId, Plan request)
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan == null) return null;

            plan.Name = request.Name;
            plan.Duration = request.Duration;
            plan.ShiftId = request.ShiftId;
            plan.BasePrice = request.BasePrice;
            plan.DiscountPercent = request.DiscountPercent;
            plan.DiscountFlat = request.DiscountFlat;

            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> DeletePlanAsync(Guid planId)
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan == null) return false;

            // Soft-delete (consistent with the sync path in UpdateLibraryAsync) - a hard
            // delete throws a DbUpdateException if this plan has ever been booked.
            plan.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Plan>> GetPlansAsync(Guid libraryId)
        {
            return await _context.Plans
                .Where(p => p.LibraryId == libraryId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Booking> CreateWalkInBookingAsync(WalkInBookingDto request)
        {
            var existingUser = await _context.EndUsers.FirstOrDefaultAsync(u => u.PhoneNumber == request.StudentContact);

            // Guard against double-booking a seat: the availability check and the insert
            // must be atomic, otherwise two concurrent walk-ins (or a walk-in racing an
            // online booking) could both pass the check before either commits.
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // A student (identified by phone number) can only hold one active/ongoing
                // plan per library at a time. "Ongoing" here means any non-cancelled booking
                // whose window hasn't fully ended yet (covers Active, Expiring, and
                // still-unpaid/pending-collection walk-ins that haven't ended) - matches the
                // frontend's "in window" concept in src/lib/status.ts.
                var today = DateTime.UtcNow.Date;
                var hasActivePlan = await _context.Bookings.AnyAsync(b =>
                    b.LibraryId == request.LibraryId &&
                    b.StudentContact == request.StudentContact &&
                    b.Status != BookingStatus.Cancelled &&
                    b.EndDate >= today);

                if (hasActivePlan)
                {
                    throw new InvalidOperationException("This student already has an active plan at this library. Cancel the existing plan or wait for it to end before adding a new one.");
                }

                // Fetch the incoming shift to check for time overlaps
                var incomingShift = await _context.ShiftTemplates.FindAsync(request.ShiftId);
                if (incomingShift == null)
                {
                    throw new InvalidOperationException("Invalid shift selected.");
                }

                var isConflict = await _context.Bookings
                    .Include(b => b.Shift)
                    .AnyAsync(b =>
                        b.SeatId == request.SeatId &&
                        b.Status != BookingStatus.Cancelled &&
                        b.StartDate < request.EndDate &&
                        b.EndDate > request.StartDate &&
                        b.Shift.StartTime < incomingShift.EndTime &&
                        b.Shift.EndTime > incomingShift.StartTime);

                if (isConflict)
                {
                    throw new InvalidOperationException("The selected seat is already booked for these dates and shift time.");
                }

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    Code = $"WI-{DateTime.UtcNow.Ticks.ToString().Substring(8)}",
                    LibraryId = request.LibraryId,
                    AreaId = request.AreaId,
                    SeatId = request.SeatId,
                    PlanId = request.PlanId,
                    ShiftId = request.ShiftId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = BookingStatus.Active,
                    Source = BookingSource.Offline,
                    PaymentMethod = PaymentMethod.PayAtLibrary, // Usually walkins pay at library
                    PaymentStatus = request.PaymentStatus,
                    PaymentDate = request.PaymentStatus == PaymentStatus.Paid ? DateTime.UtcNow : null,
                    Price = request.Price,
                    StudentName = request.StudentName,
                    StudentContact = request.StudentContact,
                    UserId = existingUser?.Id,
                    CreatedAt = DateTime.UtcNow,
                    ConfirmedArrival = true // Walk-in is already arrived
                };

                await _context.Bookings.AddAsync(booking);

                // Convert enquiry if provided
                if (request.EnquiryId.HasValue)
                {
                    var enquiry = await _context.Enquiries.FindAsync(request.EnquiryId.Value);
                    if (enquiry != null && enquiry.LibraryId == request.LibraryId)
                    {
                        enquiry.Status = "Converted";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _ruleEngine.ProcessBookingCreatedAsync(booking);

                return booking;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Booking>> GetLibraryBookingsAsync(Guid libraryId)
        {
            return await _context.Bookings
                .Include(b => b.Plan)
                .Include(b => b.Area)
                .Include(b => b.Seat)
                .Include(b => b.Shift)
                .Include(b => b.User)
                .Where(b => b.LibraryId == libraryId)
                .ToListAsync();
        }

        public async Task<Booking> GetBookingByCodeAsync(Guid ownerId, string code)
        {
            return await _context.Bookings
                .Include(b => b.Library)
                .Include(b => b.Area)
                .Include(b => b.Seat)
                .Include(b => b.Plan)
                .Include(b => b.Shift)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Code == code && b.Library.OwnerId == ownerId);
        }

        public async Task<Booking> UpdateBookingAsync(Guid bookingId, UpdateBookingDto request)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return null;

            if (request.Status.HasValue) booking.Status = request.Status.Value;
            bool becamePaid = false;
            if (request.PaymentStatus.HasValue)
            {
                if (booking.PaymentStatus != PaymentStatus.Paid && request.PaymentStatus.Value == PaymentStatus.Paid)
                {
                    booking.PaymentDate = DateTime.UtcNow;
                    becamePaid = true;
                }
                booking.PaymentStatus = request.PaymentStatus.Value;
            }
            if (request.RefundedAmount.HasValue)
            {
                // Never trust the caller's refund figure blindly - clamp it to a sane range
                // so a malformed/malicious request can't record a refund larger than what
                // was actually paid, or a negative "refund".
                var clampedRefund = Math.Max(0, Math.Min(request.RefundedAmount.Value, booking.Price));
                booking.RefundedAmount = clampedRefund;
                if (clampedRefund > 0)
                {
                    booking.PaymentStatus = PaymentStatus.Refunded;
                }
            }
            if (request.ConfirmedArrival.HasValue) booking.ConfirmedArrival = request.ConfirmedArrival.Value;

            await _context.SaveChangesAsync();
            
            if (becamePaid)
            {
                await _ruleEngine.ProcessBookingPaymentUpdatedAsync(booking);
            }

            return booking;
        }

        public Task<bool> IsLibraryOwnedByAsync(Guid libraryId, Guid ownerId) =>
            _context.Libraries.AnyAsync(l => l.Id == libraryId && l.OwnerId == ownerId);

        public Task<bool> IsAreaOwnedByAsync(Guid areaId, Guid ownerId) =>
            _context.Areas.AnyAsync(a => a.Id == areaId && a.Library.OwnerId == ownerId);

        public Task<bool> IsSeatOwnedByAsync(Guid seatId, Guid ownerId) =>
            _context.Seats.AnyAsync(s => s.Id == seatId && s.Area.Library.OwnerId == ownerId);

        public Task<bool> IsPlanOwnedByAsync(Guid planId, Guid ownerId) =>
            _context.Plans.AnyAsync(p => p.Id == planId && p.Library.OwnerId == ownerId);

        public Task<bool> IsShiftOwnedByAsync(Guid shiftId, Guid ownerId) =>
            _context.ShiftTemplates.AnyAsync(s => s.Id == shiftId && s.Library.OwnerId == ownerId);

        public Task<bool> IsBookingOwnedByAsync(Guid bookingId, Guid ownerId) =>
            _context.Bookings.AnyAsync(b => b.Id == bookingId && b.Library.OwnerId == ownerId);

        public Task<int> CountActiveBookingsAsync(Guid libraryId)
        {
            var today = DateTime.UtcNow.Date;
            return _context.Bookings.CountAsync(b =>
                b.LibraryId == libraryId &&
                b.Status != BookingStatus.Cancelled &&
                b.EndDate >= today);
        }
    }
}
