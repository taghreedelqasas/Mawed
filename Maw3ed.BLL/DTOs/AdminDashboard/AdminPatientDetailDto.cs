using System.Collections.Generic;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    // ملحوظة: مفيش حقل "العنوان" للمريض في الباك خالص (موجود بس عند الدكتور) -
    // اتشال من هنا لحد ما يتضاف عمود Address لجدول Patient/ApplicationUser لو حبيتوا.
    public class AdminPatientDetailDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Gender { get; set; } = default!;      // "ذكر" / "أنثى"
        public int Age { get; set; }
        public string RegisteredAt { get; set; } = default!; // "yyyy-MM-dd"
        public bool IsActive { get; set; }

        // نص حر زي ما هو مخزن في Patient.MedicalHistory - مش Array منظم في الباك.
        // الفرونت بيقسّمه بفواصل عشان يعرضه كـ Tags، فلو حابين دقة أعلى محتاجين
        // نحول العمود ده لجدول منفصل (ChronicDisease) بدل نص حر.
        public string? MedicalHistory { get; set; }

        public List<AdminMedicalFileDto> MedicalFiles { get; set; } = new();
    }
}
