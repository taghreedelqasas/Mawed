
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class Appointment : AuditableEntity
    {
        public int Id { get; set; }

        public AppointmentStatus Status { get; set; }

        public string? Notes { get; set; }

        public int PatientId { get; set; }

        public Patient Patient { get; set; }

        public int DoctorId { get; set; }

        public Doctor Doctor { get; set; }

        public int DoctorAvailabilityId { get; set; }

        public DoctorAvailability DoctorAvailability { get; set; }

        public Payment? Payment { get; set; }
    }
}
