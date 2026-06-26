using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Maw3ed.DAL
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SSN { get; set; }

        public DateTime BirthDate { get; set; }

        public bool IsActive { get; set; } = true;

        public Patient? Patient { get; set; }

        public Doctor? Doctor { get; set; }
    }
}
