using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class AdminPatientDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int TotalAppointments { get; set; }
    }

    public class AdminDoctorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string Address { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalAppointments { get; set; }
        public decimal Revenue { get; set; }
    }
}
