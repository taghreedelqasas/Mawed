using System;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class UpdateDoctorAvailabilityDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}