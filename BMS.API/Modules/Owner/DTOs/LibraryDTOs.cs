using System;
using System.Collections.Generic;
using BMS.API.Modules.Shared.Models;

namespace BMS.API.Modules.Owner.DTOs
{
    public class LibraryCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string AreaName { get; set; }
        public string City { get; set; }
        public string AmenitiesString { get; set; }
        public string CancellationPolicy { get; set; }
    }

    public class LibraryUpdateDto : LibraryCreateDto
    {
        public bool IsPublished { get; set; }
    }
    
    public class AreaCreateDto
    {
        public string Name { get; set; }
        public string TagsString { get; set; }
        public PriceModifierType? PriceModifierType { get; set; }
        public decimal? PriceModifierValue { get; set; }
        public string FloorPlanJson { get; set; }
    }

    public class SeatCreateDto
    {
        public string Number { get; set; }
        public GenderRestriction GenderRestriction { get; set; }
        public decimal? PriceOverride { get; set; }
    }

    public class WalkInBookingDto
    {
        public Guid LibraryId { get; set; }
        public Guid AreaId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PlanId { get; set; }
        public Guid ShiftId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StudentName { get; set; }
        public string StudentContact { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
