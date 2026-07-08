using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class KpiCardDto
    {
        // القيمة الحالية (ممكن تبقى عدد أو مبلغ فلوس)
        public decimal Value { get; set; }

        // نسبة التغيير مقارنة بالفترة اللي قبلها (موجب = زيادة، سالب = نقصان)
        public decimal ChangePercentage { get; set; }

        // "الشهر الماضي" أو "أمس" ...إلخ
        public string ComparisonLabel { get; set; } = string.Empty;

        // قيمة الفترة اللي قبلها عشان تتعرض جنب الـ label
        public decimal ComparisonValue { get; set; }
    }
}
