using System;
using System.Collections.Generic;

namespace Maw3ed.BLL.DTOs.AdminDashboard
{
    public class AdminAppointmentsPagedResultDto
    {
        public List<AdminAppointmentDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
