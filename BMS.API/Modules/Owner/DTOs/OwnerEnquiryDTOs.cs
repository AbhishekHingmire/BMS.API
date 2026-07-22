using System;
using System.ComponentModel.DataAnnotations;
using BMS.API.Modules.Shared.Models;

namespace BMS.API.Modules.Owner.DTOs
{
    public class EnquiryDto
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public string StudentName { get; set; }
        public string StudentContact { get; set; }
        public string StudentEmail { get; set; }
        public string Gender { get; set; }
        public string Notes { get; set; }
        public EnquiryStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateEnquiryDto
    {
        [Required]
        public Guid LibraryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StudentName { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public string StudentContact { get; set; }

        [MaxLength(100)]
        public string StudentEmail { get; set; }

        [MaxLength(20)]
        public string Gender { get; set; }

        public string Notes { get; set; }
    }

    public class UpdateEnquiryStatusDto
    {
        [Required]
        public EnquiryStatus Status { get; set; }
    }
}
