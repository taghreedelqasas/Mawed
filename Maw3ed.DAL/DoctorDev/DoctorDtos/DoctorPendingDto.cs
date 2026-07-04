using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class DoctorPendingDto
    {
        public string UserId { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string LicenseNumber { get; set; } = default!;
        public string Certificate { get; set; } = default!;
        public decimal ConsultationFee { get; set; }
        public string Address { get; set; } = default!;
        public DateTime GraduationDate { get; set; }
        public int DepartmentId { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
