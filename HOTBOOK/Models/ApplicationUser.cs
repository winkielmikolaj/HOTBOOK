using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace HOTBOOK.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Booking> Bookings { get; set; }
    }
} 