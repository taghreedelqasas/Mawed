using Maw3ed.DAL.DoctorDev.DoctorDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; // إضافة المكتبة

namespace Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces
{
    public interface IDoctorAvailabilityManager
    {
        Task AddAsync(CreateDoctorAvailabilityDto dto);
        Task BulkAddAsync(BulkCreateDoctorAvailabilityDto dto);
        Task UpdateAsync(UpdateDoctorAvailabilityDto dto);
        Task<List<DoctorAvailability>> GetByDoctorAsync(int doctorId);
        Task<List<DoctorAvailability>> GetAvailableSlotsAsync(int doctorId, DateTime? date = null);
        Task<DoctorAvailability> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}