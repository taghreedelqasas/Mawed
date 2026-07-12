using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class DoctorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Specialty { get; set; }
        public string Location { get; set; }
        public string? ImageProfile { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalPatients { get; set; }
        public decimal ConsultationFee { get; set; }
        public string Certificate { get; set; }
        public int YearsOfExperience { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailableToday { get; set; }
        public List<AvailableSlotDto> TodaySlots { get; set; } = new();
    }
    public class AvailableSlotDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
