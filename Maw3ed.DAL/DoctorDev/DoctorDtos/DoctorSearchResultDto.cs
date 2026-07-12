using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class DoctorSearchResultDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Specialty { get; set; }
        public string Location { get; set; }
        public string? ImageProfile { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailableToday { get; set; }
    }
}
