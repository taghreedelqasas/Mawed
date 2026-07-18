namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    // ملحوظة مهمة: التصميم الأصلي فيه كارت "مؤجلة" (Postponed)، لكن الـ AppointmentStatus
    // enum الحالي (Pending, Confirmed, Completed, Cancelled) مفيهوش حالة "مؤجلة" خالص.
    // الكروت هنا مبنية على الحالات الحقيقية الموجودة فعلاً + عدد حجوزات اليوم.
    // لو "مؤجلة" مطلوبة كمفهوم منفصل (إعادة جدولة الموعد)، محتاجة تتضاف كحالة جديدة
    // في الـ Enum ويتحدث الـ Business logic اللي بيغيّر حالة الموعد.
    public class AdminAppointmentsSummaryDto
    {
        public int TodayBookings { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
