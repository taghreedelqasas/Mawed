using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{

    public class PlatformSetting
    {
        public int Id { get; set; }

        // نسبة العمولة كنسبة مئوية (مثال: 10 يعني 10%)
        public decimal CommissionRate { get; set; } = 10m;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedBy { get; set; }
    }
}
