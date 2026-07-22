using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Owner.DTOs
{
    public class AttendanceRecordDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid LibraryId { get; set; }
        public DateTime Date { get; set; }
        public DateTime MarkedAt { get; set; }
    }

    public class MarkAttendanceDto
    {
        [Required]
        public Guid BookingId { get; set; }

        /// <summary>Optional - defaults to today (UTC) when omitted.</summary>
        public DateTime? Date { get; set; }
    }
}
