using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class MonthlyTrendPointDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; } = string.Empty;  // "يون", "يوليو" ...إلخ
        public decimal Value { get; set; }
    }

    // شكل الـ Response بتاع GET /api/admin/dashboard/monthly-trend?type=appointments|revenue
    public class MonthlyTrendDto
    {
        public string Type { get; set; } = string.Empty; // appointments | revenue
        public List<MonthlyTrendPointDto> Points { get; set; } = new();
    }
}
