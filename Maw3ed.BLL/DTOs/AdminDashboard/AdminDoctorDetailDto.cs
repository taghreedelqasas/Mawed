namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class AdminDoctorDetailDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Gender { get; set; } = default!; // "ذكر" / "أنثى"
        public int Age { get; set; }
        public string RegisteredAt { get; set; } = default!; // "yyyy-MM-dd"
        public bool IsActive { get; set; }

        public string Department { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string LicenseNumber { get; set; } = default!;
        public decimal ConsultationFee { get; set; }
        public string GraduationDate { get; set; } = default!; // "yyyy-MM-dd"
        public bool IsVerified { get; set; }

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalAppointments { get; set; }

        public string? LicenseImage { get; set; }
        public string? SSNImg { get; set; }
        public string? CertificateImage { get; set; }
    }
}
