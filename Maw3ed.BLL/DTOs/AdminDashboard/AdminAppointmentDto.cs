namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class AdminAppointmentDto
    {
        public int Id { get; set; }

        // مبني من الـ Id الداخلي حاليًا (A-{1000+Id}) - لو محتاجين تسلسل حجز مستقل
        // عن الـ Id لازم يتضاف عمود جديد في جدول Appointments.
        public string BookingNumber { get; set; } = default!;

        public string DoctorName { get; set; } = default!;
        public string PatientName { get; set; } = default!;

        // من Payment.Amount لو الدفع اتعمل، وإلا بيرجع لـ Doctor.ConsultationFee كقيمة متوقعة
        public decimal Fee { get; set; }

        public string Date { get; set; } = default!; // "yyyy-MM-dd"
        public string Time { get; set; } = default!; // "10:00 ص"

        public string Status { get; set; } = default!;      // Pending / Confirmed / Completed / Cancelled
        public string StatusLabel { get; set; } = default!;  // قيد الانتظار / مؤكد / مكتمل / ملغي
    }
}
