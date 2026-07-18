using System;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class AdminMedicalFileDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = default!;
        public string FileUrl { get; set; } = default!;
        public string FileType { get; set; } = default!;  // "PDF" / "JPEG" ...
        public string CategoryLabel { get; set; } = default!; // تحليل / أشعة / وصفة / تقرير
        public string UploadedAt { get; set; } = default!; // "yyyy-MM-dd"
    }
}
