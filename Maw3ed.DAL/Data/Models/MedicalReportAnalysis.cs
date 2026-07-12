using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
    public class MedicalReportAnalysis : AuditableEntity
    {
        public int Id { get; set; }

        public int? PatientId { get; set; }

        public Patient? Patient { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ReportText { get; set; } = string.Empty;

        public string AIExplanation { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;
    }
}
