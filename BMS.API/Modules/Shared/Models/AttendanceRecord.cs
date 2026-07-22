using System;

namespace BMS.API.Modules.Shared.Models
{
    /// <summary>
    /// One attendance mark for a booking on a given calendar date. Unique on (BookingId, Date)
    /// so a student can only be marked present once per day for a given booking.
    /// </summary>
    public class AttendanceRecord
    {
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        public Guid LibraryId { get; set; }

        /// <summary>Calendar date (time component always midnight UTC) the attendance is for.</summary>
        public DateTime Date { get; set; }

        public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
