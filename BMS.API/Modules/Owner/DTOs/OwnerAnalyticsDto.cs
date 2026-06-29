using System;
using System.Collections.Generic;

namespace BMS.API.Modules.Owner.DTOs
{
    public class OwnerAnalyticsDto
    {
        public decimal TodaysRevenue { get; set; }
        public decimal SevenDaysRevenue { get; set; }
        public decimal ThirtyDaysRevenue { get; set; }
        public int ActiveBookingsCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public decimal PendingCollectionAmount { get; set; }
        public int PendingCollectionCount { get; set; }
        
        public List<DailyMetricDto> DailyMetrics { get; set; } = new();
        public List<PaymentMethodSplitDto> PaymentMethodSplit { get; set; } = new();
        public List<PlanPopularityDto> PlanPopularity { get; set; } = new();

        public int OccupiedNowCount { get; set; }
        public int PendingArrivalCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public List<ExpiringMembershipDto> ExpiringMemberships { get; set; } = new();
    }

    public class ExpiringMembershipDto
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; }
        public string StudentContact { get; set; }
        public string LibraryName { get; set; }
        public string EndDate { get; set; }
        public string PlanName { get; set; }
    }

    public class DailyMetricDto
    {
        public string Date { get; set; }
        public string Label { get; set; }
        public decimal Online { get; set; }
        public decimal Offline { get; set; }
        public decimal Total { get; set; }
        public int Occupancy { get; set; } // Percentage 0-100
    }

    public class PaymentMethodSplitDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }

    public class PlanPopularityDto
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
