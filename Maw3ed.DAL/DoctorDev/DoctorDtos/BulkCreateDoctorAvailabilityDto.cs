using System;
using System.Collections.Generic;

namespace Maw3ed.DAL.DoctorDev.DoctorDtos
{
    public class BulkCreateDoctorAvailabilityDto
    {
        public int DoctorId { get; set; }
        public List<DaySlotDto> Days { get; set; } = new List<DaySlotDto>();
    }

    public class DaySlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}