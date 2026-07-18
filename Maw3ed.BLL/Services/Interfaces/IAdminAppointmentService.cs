using Maw3ed.BLL.DTOs.AdminDashboard;
using System;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IAdminAppointmentService
    {
        Task<AdminAppointmentsPagedResultDto> GetAppointmentsAsync(
            int page, int pageSize, string? status, DateTime? date, string? search);

        Task<AdminAppointmentsSummaryDto> GetSummaryAsync();
    }
}
