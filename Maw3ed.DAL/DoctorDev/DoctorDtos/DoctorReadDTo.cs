using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos 
{
    public class DoctorReadDTo
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public string Address { get; set; }
        public bool IsVerified { get; set; }
        public string DepartmentName { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ImageProfile { get; set; }
    }
}