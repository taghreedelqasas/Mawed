using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class DashboardOverviewDto
    {
        public KpiCardDto TodayAppointments { get; set; } = new();     // مواعيد اليوم
        public KpiCardDto TotalAppointments { get; set; } = new();     // إجمالي المواعيد
        public KpiCardDto TotalPatients { get; set; } = new();         // إجمالي المرضى
        public KpiCardDto TotalDoctors { get; set; } = new();          // إجمالي الأطباء
        public KpiCardDto CompletionRate { get; set; } = new();        // معدل الإتمام (%)
        public KpiCardDto PlatformCommission { get; set; } = new();    // عمولة المنصة
        public KpiCardDto TotalRevenue { get; set; } = new();          // إجمالي الإيرادات
        public KpiCardDto ActiveConsultations { get; set; } = new();   // الاستشارات النشطة
    }
}
