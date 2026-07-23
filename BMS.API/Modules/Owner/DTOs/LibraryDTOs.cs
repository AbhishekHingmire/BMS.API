using System;
using System.ComponentModel.DataAnnotations;
using BMS.API.Modules.Shared.Models;

namespace BMS.API.Modules.Owner.DTOs
{
    public class LibraryCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string AreaName { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        public string? AmenitiesString { get; set; }
        
        public System.Collections.Generic.List<string> Photos { get; set; }

        [StringLength(2000)]
        public string? CancellationPolicy { get; set; }

        public string? FaqJson { get; set; }

        public System.Collections.Generic.List<AreaCreateDto> Areas { get; set; } = new();
        public System.Collections.Generic.List<ShiftCreateDto> Shifts { get; set; } = new();
        public System.Collections.Generic.List<PlanCreateDto> Plans { get; set; } = new();
        
        public bool IsPublished { get; set; }
    }

    public class LibraryUpdateDto : LibraryCreateDto
    {
    }
    
    public class AreaCreateDto
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string? TagsString { get; set; }
        public PriceModifierType? PriceModifierType { get; set; }
        public decimal? PriceModifierValue { get; set; }
        public string? FloorPlanJson { get; set; }
        public string? PlanOverrideIdsJson { get; set; }

        public System.Collections.Generic.List<SeatCreateDto> Seats { get; set; } = new();
    }

    public class SeatCreateDto
    {
        public Guid? Id { get; set; }

        [Required]
        public string Number { get; set; }
        
        public GenderRestriction GenderRestriction { get; set; }
        public decimal? PriceOverride { get; set; }
    }

    public class ShiftCreateDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class PlanCreateDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public DurationType Duration { get; set; }
        public Guid? ShiftId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountFlat { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? DaysOfWeekString { get; set; }
    }

    public class WalkInBookingDto
    {
        [Required]
        public Guid LibraryId { get; set; }
        [Required]
        public Guid AreaId { get; set; }
        [Required]
        public Guid SeatId { get; set; }
        [Required]
        public Guid PlanId { get; set; }
        [Required]
        public Guid ShiftId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public string StudentName { get; set; }
        [Required]
        public string StudentContact { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Price { get; set; }
        public Guid? EnquiryId { get; set; }
    }

    public class UpdateBookingDto
    {
        public BookingStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public bool? ConfirmedArrival { get; set; }
        public decimal? RefundedAmount { get; set; }
    }

    // Response DTOs to match frontend types exactly
    public class SeatResponseDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string? GenderRestriction { get; set; }
        public decimal? PriceOverride { get; set; }
        public bool Inactive { get; set; }
    }

    public class ShiftResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Start { get; set; } // "HH:mm"
        public string End { get; set; }
    }

    public class PlanResponseDto
    {
        public Guid Id { get; set; }
        public string Duration { get; set; } // "daily", "monthly", etc.
        public Guid? ShiftId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountFlat { get; set; }
        public string? Name { get; set; }
        public bool Enabled { get; set; }
        public List<int> DaysOfWeek { get; set; } = new();
    }

    public class AreaResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; } = new();
        public object? PriceModifier { get; set; } // { type, value }
        public List<Guid> PlanOverrideIds { get; set; } = new();
        public object? FloorPlan { get; set; } // JSON deserialized
        public List<SeatResponseDto> Seats { get; set; } = new();
    }

    public class LibraryResponseDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public List<string> Photos { get; set; } = new();
        public List<string> Amenities { get; set; } = new();
        public bool Verified { get; set; }
        public bool Published { get; set; }
        public string CancellationPolicy { get; set; }
        public string FaqJson { get; set; } = "[]";

        // Set when the server force-unpublished the library as a result of this update
        // (e.g. an area/seat/shift/plan deletion left it without any bookable seats,
        // shifts, or enabled plans). The owner UI surfaces this as a warning toast.
        public string? AutoUnpublishedReason { get; set; }
        
        public List<ShiftResponseDto> Shifts { get; set; } = new();
        public List<AreaResponseDto> Areas { get; set; } = new();
        public List<PlanResponseDto> Plans { get; set; } = new();
    }
}
