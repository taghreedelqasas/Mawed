using Maw3ed.BLL.DTOs.AdminDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        // الكروت الـ 8 اللي فوق (نظرة عامة)
        Task<DashboardOverviewDto> GetOverviewAsync();

        // دونات توزيع حالات المواعيد لشهر معين (افتراضيًا الشهر الحالي)
        Task<AppointmentStatusDistributionDto> GetAppointmentStatusDistributionAsync(int? year, int? month);

        // خط اتجاه المواعيد أو الإيرادات آخر N شهر (افتراضيًا 12)
        Task<MonthlyTrendDto> GetMonthlyTrendAsync(string type, int months);

        Task<IEnumerable<AdminPatientDto>> GetAllPatientsAsync();
        Task<IEnumerable<AdminDoctorDto>> GetAllDoctorsAsync();


        // جديد: بروفايل مريض/طبيب كامل لباتش "عرض الملف الشخصي"
        Task<AdminPatientDetailDto?> GetPatientDetailAsync(int patientId);
        Task<AdminDoctorDetailDto?> GetDoctorDetailAsync(int doctorId);
    }
}
