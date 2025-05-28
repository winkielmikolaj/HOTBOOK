using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HOTBOOK.Models
{
    public class Guest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [Required]
        public string PassportNumber { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
} 