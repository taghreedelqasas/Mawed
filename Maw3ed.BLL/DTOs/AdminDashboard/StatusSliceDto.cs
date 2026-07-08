using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class StatusSliceDto
    {
        public string Status { get; set; } = string.Empty;   // Completed / Pending / Confirmed / Cancelled
        public string Label { get; set; } = string.Empty;    // مكتمل / قيد الانتظار / مؤكد / ملغي
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    // شكل الـ Response بتاع GET /api/admin/dashboard/appointments-distribution
    public class AppointmentStatusDistributionDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalAppointments { get; set; }

        // نسبة "مكتمل" اللي بتتعرض في نص الدونات (68% مكتمل)
        public decimal CompletedPercentage { get; set; }

        public List<StatusSliceDto> Slices { get; set; } = new();
    }
}
