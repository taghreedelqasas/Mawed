using Maw3ed.BLL.DTOs.AdminDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminPayments
{
    public class PaymentsSummaryDto
    {
        // المدفوعات المعلقة: مجموع المدفوعات اللي لسه Pending (ماتحصّلتش لحد دلوقتي)
        public KpiCardDto PendingPayments { get; set; } = new();

        // أرباح الأطباء: صافي المبلغ بعد خصم عمولة المنصة (من المدفوعات الناجحة/Paid)
        public KpiCardDto DoctorsProfit { get; set; } = new();

        // عمولة المنصة: مجموع SystemFee من المدفوعات الناجحة
        public KpiCardDto PlatformCommission { get; set; } = new();

        // إجمالي الإيرادات: مجموع كل المدفوعات الناجحة (Amount)
        public KpiCardDto TotalRevenue { get; set; } = new();

        // نسبة العمولة الحالية (تُعرض جنب كارت "عمولة المنصة" كنسبة %)
        public decimal CommissionRate { get; set; }
    }
}
