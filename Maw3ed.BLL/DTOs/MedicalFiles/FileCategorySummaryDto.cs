using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.MedicalFiles
{
    public class FileCategorySummaryDto
    {
        public int LabResultCount { get; set; }      // تحاليل طبية
        public int ScanCount { get; set; }           // أشعة
        public int PrescriptionCount { get; set; }   // وصفات طبية
        public int MedicalReportCount { get; set; }  // تقارير طبية
        public int TotalCount { get; set; }          // إجمالي
    }
}
