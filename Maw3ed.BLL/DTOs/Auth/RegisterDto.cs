using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Maw3ed.BLL
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
        public DateTime BirthDate { get; set; }
        public string SSN { get; set; } = default!;

        // "Patient" or "Doctor" - validated against allowed roles in the Validator.
        public string Role { get; set; } = default!;

        //url from postman
        public string ClientBaseUrl { get; set; } = default!;

        // ---- Only required when Role = "Doctor" ----
        public string? LicenseNumber { get; set; }
        public string? Certificate { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? Address { get; set; }
        public DateTime? GraduationDate { get; set; }
        public int? DepartmentId { get; set; }

       
    }
}
