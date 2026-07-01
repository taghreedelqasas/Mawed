namespace Maw3ed.BLL.DTOs.Appointment
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;

        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
