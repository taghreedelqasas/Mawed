using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminPayments
{
    public class CommissionRateDto
    {
        // نسبة العمولة كنسبة مئوية (مثال: 10 يعني 10%)
        public decimal CommissionRate { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
    }

    public class UpdateCommissionRateDto
    {
        // نسبة العمولة الجديدة كنسبة مئوية (من 0 لـ 100)
        public decimal CommissionRate { get; set; }
    }
}
