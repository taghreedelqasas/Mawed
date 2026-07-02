using Maw3ed.DAL.Reposatries.Interfaces;
using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorManager
{
    public class DoctorManagerClasses : IDoctorManager
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorManagerClasses(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DoctorReadDTo>> GetAllAsync()
        {
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(null, d => d.Department, d => d.User);

            return doctors.Select(d => new DoctorReadDTo
            {
                Id = d.Id,
                LicenseNumber = d.LicenseNumber,
                ConsultationFee = d.ConsultationFee,
                Address = d.Address,
                IsVerified = d.IsVerified,
                DepartmentName = d.Department?.Name ?? "",
                UserName = d.User?.UserName ?? " ",
                ImageProfile = d.ImageProfile // تعديل هنا لعرض الصورة
            });
        }

        public async Task<DoctorReadDTo?> GetByIdAsync(int id)
        {
            var doctor = await _unitOfWork
                .GetRepository<Doctor>()
                .GetByIdAsync(id);

            if (doctor == null)
                return null;

            return new DoctorReadDTo
            {
                Id = doctor.Id,
                LicenseNumber = doctor.LicenseNumber,
                ConsultationFee = doctor.ConsultationFee,
                Address = doctor.Address,
                IsVerified = doctor.IsVerified,
                DepartmentName = doctor.Department?.Name ?? "",
                UserName = doctor.User?.UserName ?? "",
                ImageProfile = doctor.ImageProfile // تعديل هنا لعرض الصورة
            };
        }

        public async Task AddAsync(DoctorCreateDto doctorDto)
        {
            var doctor = new Doctor
            {
                LicenseNumber = doctorDto.LicenseNumber,
                Certificate = doctorDto.Certificate,
                ConsultationFee = doctorDto.ConsultationFee,
                Address = doctorDto.Address,
                GraduationDate = doctorDto.GraduationDate,
                UserId = doctorDto.UserId,
                DepartmentId = doctorDto.DepartmentId,
                IsVerified = false,
                ImageProfile = doctorDto.ImageProfile // تعديل هنا لحفظ الصورة الجديدة
            };

           await  _unitOfWork.GetRepository<Doctor>().AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(DoctorUpdateDto doctorDto)
        {
            var existingDoctor = await _unitOfWork.GetRepository<Doctor>().GetByIdAsync(doctorDto.Id);

            if (existingDoctor == null)
                throw new Exception("Doctor not found");

            existingDoctor.LicenseNumber = doctorDto.LicenseNumber;
            existingDoctor.Certificate = doctorDto.Certificate;
            existingDoctor.ConsultationFee = doctorDto.ConsultationFee;
            existingDoctor.Address = doctorDto.Address;
            existingDoctor.GraduationDate = doctorDto.GraduationDate;
            existingDoctor.DepartmentId = doctorDto.DepartmentId;

            // تعديل هنا لتحديث مسار الصورة الجديدة في حال تم تغييرها
            existingDoctor.ImageProfile = doctorDto.ImageProfile;

            _unitOfWork.GetRepository<Doctor>().Update(existingDoctor);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var doctor = await _unitOfWork.GetRepository<Doctor>().GetByIdAsync(id);

            if (doctor != null)
            {
                _unitOfWork.GetRepository<Doctor>().Delete(doctor);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}