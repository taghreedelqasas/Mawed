namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class DoctorUpdateDto
    {
        public int Id { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Certificate { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? Address { get; set; }
        public DateTime? GraduationDate { get; set; }
        public int? DepartmentId { get; set; }
        public string? ImageProfile { get; set; }
    }
}