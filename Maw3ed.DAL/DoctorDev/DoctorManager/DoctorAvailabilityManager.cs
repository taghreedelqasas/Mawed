using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Maw3ed.DAL.Reposatries.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // إضافة المكتبة

namespace Maw3ed.DAL.DoctorDev.DoctorManager
{
    public class DoctorAvailabilityManager : IDoctorAvailabilityManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int SlotDuration = 20;

        public DoctorAvailabilityManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddAsync(CreateDoctorAvailabilityDto dto) // تعديل هنا
        {
            await AddSingleDayAsync(dto.DoctorId, dto.StartTime, dto.EndTime); // تعديل هنا
            await _unitOfWork.SaveChangesAsync(); // تعديل هنا
        }

        public async Task<List<DoctorAvailability>> GetByDoctorAsync(int doctorId) // تعديل هنا
        {
            var result = await _unitOfWork
                .GetRepository<DoctorAvailability>()
                .GetAllAsync(x => x.DoctorId == doctorId); // تعديل هنا

            return result.OrderBy(x => x.StartTime).ToList();
        }

        public async Task<List<DoctorAvailability>> GetAvailableSlotsAsync(int doctorId, DateTime? date = null) // تعديل هنا
        {
            var result = await _unitOfWork
                .GetRepository<DoctorAvailability>()
                .GetAllAsync(x =>
                    x.DoctorId == doctorId &&
                    !x.IsBooked &&
                    (date == null || x.StartTime.Date == date.Value.Date)); // تعديل هنا

            return result.OrderBy(x => x.StartTime).ToList();
        }

        public async Task<DoctorAvailability> GetByIdAsync(int id) // تعديل هنا
        {
            var slot = await _unitOfWork.GetRepository<DoctorAvailability>().GetByIdAsync(id); // تعديل هنا

            if (slot == null)
                throw new Exception("Availability slot not found.");

            return slot;
        }

        public async Task DeleteAsync(int id) // تعديل هنا
        {
            var slot = await GetByIdAsync(id); // تعديل هنا

            if (slot.IsBooked)
                throw new Exception("Cannot delete a booked slot.");

            _unitOfWork.GetRepository<DoctorAvailability>().Delete(slot);
            await _unitOfWork.SaveChangesAsync(); // تعديل هنا
        }

        public async Task BulkAddAsync(BulkCreateDoctorAvailabilityDto dto) // تعديل هنا
        {
            if (dto.Days == null || !dto.Days.Any())
                throw new Exception("At least one day must be provided.");

            foreach (var day in dto.Days)
            {
                await AddSingleDayAsync(dto.DoctorId, day.StartTime, day.EndTime); // تعديل هنا
            }

            await _unitOfWork.SaveChangesAsync(); // تعديل هنا
        }

        // جعلنا هذه الدالة المساعدة async لأنها تقوم بطلب جلب بيانات من الداتابيز
        private async Task AddSingleDayAsync(int doctorId, DateTime startTime, DateTime endTime) // تعديل هنا
        {
            if (startTime >= endTime)
                throw new Exception($"Start time must be before end time for {startTime.Date:yyyy-MM-dd}.");

            var duration = (endTime - startTime).TotalMinutes;

            if (duration % SlotDuration != 0)
                throw new Exception($"Duration must be divisible by 20 minutes for {startTime.Date:yyyy-MM-dd}.");

            var existingSlots = await _unitOfWork
                .GetRepository<DoctorAvailability>()
                .GetAllAsync(x =>
                    x.DoctorId == doctorId &&
                    x.StartTime.Date == startTime.Date); // تعديل هنا

            foreach (var slot in existingSlots)
            {
                if (startTime < slot.EndTime && endTime > slot.StartTime)
                {
                    throw new Exception($"Availability overlaps with an existing slot on {startTime.Date:yyyy-MM-dd}.");
                }
            }

            var currentStart = startTime;

            while (currentStart < endTime)
            {
                var currentEnd = currentStart.AddMinutes(SlotDuration);

                var availability = new DoctorAvailability
                {
                    DoctorId = doctorId,
                    StartTime = currentStart,
                    EndTime = currentEnd,
                    IsBooked = false
                };

                 await  _unitOfWork.GetRepository<DoctorAvailability>().AddAsync(availability);

                currentStart = currentEnd;
            }
        }

        public async Task UpdateAsync(UpdateDoctorAvailabilityDto dto) // تعديل هنا
        {
            var slot = await _unitOfWork.GetRepository<DoctorAvailability>().GetByIdAsync(dto.Id); // تعديل هنا

            if (slot == null)
                throw new Exception("Availability slot not found.");

            if (slot.IsBooked)
                throw new Exception("Cannot update a booked slot.");

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Start time must be before end time.");

            var duration = (dto.EndTime - dto.StartTime).TotalMinutes;

            if (duration != SlotDuration)
                throw new Exception("Slot duration must be exactly 20 minutes.");

            var existingSlots = await _unitOfWork
                .GetRepository<DoctorAvailability>()
                .GetAllAsync(x =>
                    x.DoctorId == slot.DoctorId &&
                    x.Id != dto.Id &&
                    x.StartTime.Date == dto.StartTime.Date); // تعديل هنا

            foreach (var s in existingSlots)
            {
                if (dto.StartTime < s.EndTime && dto.EndTime > s.StartTime)
                    throw new Exception("This availability overlaps with an existing slot.");
            }

            slot.StartTime = dto.StartTime;
            slot.EndTime = dto.EndTime;

            _unitOfWork.GetRepository<DoctorAvailability>().Update(slot);
            await _unitOfWork.SaveChangesAsync(); // تعديل هنا
        }
    }
}