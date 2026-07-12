using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class DoctorSearchFilterDto
    {
        public string? Specialty { get; set; }
        public string? Location { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? Date { get; set; }
        public string? SortBy { get; set; } // rating, price, available
    }
}
