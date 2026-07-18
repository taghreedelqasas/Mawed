using Maw3ed.DAL.DoctorDev.DoctorDtos;
using System.Collections.Generic;
using System.Threading.Tasks; // إضافة المكتبة
namespace Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces
{
    public interface IDoctorManager
    {
        Task<IEnumerable<DoctorReadDTo>> GetAllAsync();
        Task<List<DoctorPendingDto>> GetPendingDoctorsAsync();
        Task<DoctorReadDTo?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> ApproveDoctorAsync(string userId);
        Task<(bool Success, string Message)> RejectDoctorAsync(string userId, string? reason = null);
        Task AddAsync(DoctorCreateDto doctor);

        Task UpdateAsync(DoctorUpdateDto doctor);
  
        Task DeleteAsync(int id);
        Task<DoctorReadDTo?> GetByUserIdAsync(string userId);
        Task UpdateOwnProfileAsync(string userId, DoctorUpdateDto doctorDto);
    }
}