using Maw3ed.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class Doctor : AuditableEntity
    {
        public int Id { get; set; }

        public string LicenseNumber { get; set; }

        public string Certificate { get; set; }

        public decimal ConsultationFee { get; set; }

        public string Address { get; set; }

        public bool IsVerified { get; set; }
        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime GraduationDate { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int DepartmentId { get; set; }

        public Department Department { get; set; }

        public ICollection<DoctorAvailability> Availabilities { get; set; }
            = new List<DoctorAvailability>();

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();

        public ICollection<Conversation> Conversations { get; set; }
      = new List<Conversation>();

    }
}
