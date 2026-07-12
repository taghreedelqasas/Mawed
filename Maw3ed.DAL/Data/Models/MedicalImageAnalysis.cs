using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
    public class MedicalImageAnalysis : AuditableEntity
    {
        public int Id { get; set; }

        public int? PatientId { get; set; }

        public Patient? Patient { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        // Public URL that AI will access
        public string ImageUrl { get; set; } = string.Empty;

        public string AIExplanation { get; set; } = string.Empty;
    }
}
