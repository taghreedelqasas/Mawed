namespace Maw3ed.BLL.DTOs.Appointment
{
    public class BookAppointmentDto
    {
        public int DoctorAvailabilityId { get; set; }
        public string? Notes { get; set; }

        public string? PatientFullName { get; set; }
        public string? PatientPhone { get; set; }
        public string? PatientGender { get; set; }
        public int? PatientAge { get; set; }
    }
}
