using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.Shared.Models
{
    public class Enquiry
    {
        public Guid Id { get; set; }

        public Guid LibraryId { get; set; }

        [MaxLength(50)]
        public string StudentName { get; set; }

        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public string StudentContact { get; set; }

        [MaxLength(100)]
        public string? StudentEmail { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        public string? Notes { get; set; }

        public EnquiryStatus Status { get; set; } = EnquiryStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Library Library { get; set; }
    }
}
