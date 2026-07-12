using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Maw3ed.DAL
{
    public enum Gender
    {
        Male = 1,
        Female = 2,
    }
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SSN { get; set; }

        public DateTime BirthDate { get; set; }
        public string? ProfilePictureUrl { get; set; } // ← الحقل الجديد
        public Gender Gender { get; set; }


        public bool IsActive { get; set; } = true;

        public Patient? Patient { get; set; }

        public Doctor? Doctor { get; set; }
    }
}
