using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
    public class MedicalFile : AuditableEntity
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public MedicalFileCategory Category { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedAtUtc { get; set; }

        public string? OcrProcessedText { get; set; }
        public OcrStatus OcrStatus { get; set; } = OcrStatus.NotStarted;
    }

    public enum MedicalFileCategory
    {
        LabResult,       // تحاليل طبية
        Scan,            // أشعة
        Prescription,    // وصفات طبية
        MedicalReport    // تقارير طبية
    }

    public enum OcrStatus
    {
        NotStarted,
        Pending,
        Processed,
        Failed
    }
}
