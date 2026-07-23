using System;
using System.ComponentModel.DataAnnotations;

namespace BMS.API.Modules.SuperAdmin.DTOs
{
    public class SuperAdminUpdateOwnerDto
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string PhoneNumber { get; set; }

        public string Password { get; set; } // Optional, if empty doesn't update
    }

    public class SuperAdminAllocatePlanDto
    {
        [Required]
        public string Plan { get; set; }
    }

    public class SuperAdminNotifyDto
    {
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
    }
}
