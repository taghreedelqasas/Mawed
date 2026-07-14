using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminPayments
{
    public class RevenueCommissionPointDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        // اسم الشهر بالعربي زي "يناير"
        public string Label { get; set; } = string.Empty;

        // إجمالي الإيرادات (Amount) للمدفوعات الناجحة في الشهر ده
        public decimal Revenue { get; set; }

        // إجمالي عمولة المنصة (SystemFee) للمدفوعات الناجحة في الشهر ده
        public decimal Commission { get; set; }
    }

    public class RevenueCommissionTrendDto
    {
        public List<RevenueCommissionPointDto> Points { get; set; } = new();
    }
}
