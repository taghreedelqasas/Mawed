using Maw3ed.DAL.DoctorDev.DoctorDtos;
using System.Collections.Generic;
using System.Threading.Tasks; // إضافة المكتبة

namespace Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces
{
    public interface IDoctorManager
    {
        Task<IEnumerable<DoctorReadDTo>> GetAllAsync();

        Task<DoctorReadDTo?> GetByIdAsync(int id);

        Task AddAsync(DoctorCreateDto doctor);

        Task UpdateAsync(DoctorUpdateDto doctor);

        Task DeleteAsync(int id);
    }
}