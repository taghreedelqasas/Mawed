using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Maw3ed.DAL.Reposatries.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maw3ed.BLL
{
    public class DoctorManager : IDoctorManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public DoctorManager(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
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
                FirstName = d.User?.FirstName,
                LastName = d.User?.LastName,
                ImageProfile = d.ImageProfile
            });
        }

        public async Task<DoctorReadDTo?> GetByIdAsync(int id)
        {
            // استخدام GetAllAsync بالـ includes بدل GetByIdAsync العادية
            // عشان نضمن إن Department و User مش هيبقوا null
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(d => d.Id == id, d => d.Department, d => d.User);

            var doctor = doctors.FirstOrDefault();

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
                FirstName = doctor.User?.FirstName,
                LastName = doctor.User?.LastName,
                ImageProfile = doctor.ImageProfile
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
                ImageProfile = doctorDto.ImageProfile
            };

            await _unitOfWork.GetRepository<Doctor>().AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            var wallet = new DoctorWallet
            {
                DoctorId = doctor.Id,
                Balance = 0,
                PendingBalance = 0
            };

            await _unitOfWork.GetRepository<DoctorWallet>().AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<(bool Success, string Message)> RejectDoctorAsync(string userId, string? reason = null)
        {
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(d => d.UserId == userId, d => d.User);

            var doctor = doctors.FirstOrDefault();

            if (doctor is null)
                return (false, "Doctor not found.");

            if (doctor.IsVerified)
                return (false, "Cannot reject a doctor who is already approved.");

            var email = doctor.User.Email!;
            var name = $"{doctor.User.FirstName} {doctor.User.LastName}";

            // Remove the pending doctor request.
            _unitOfWork.GetRepository<Doctor>().Delete(doctor);

            // Deactivate the linked account so they can't log in with a rejected request,
            // while keeping the email history for auditing.
            doctor.User.IsActive = false;
            await _unitOfWork.AuthRepository.UpdateUserAsync(doctor.User);

            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendDoctorRejectionAsync(
                toEmail: email,
                toName: name,
                reason: reason);

            return (true, "Doctor rejected successfully.");
        }
        public async Task UpdateAsync(DoctorUpdateDto doctorDto)
        {
            var existingDoctor = await _unitOfWork.GetRepository<Doctor>().GetByIdAsync(doctorDto.Id);
            if (existingDoctor == null)
                throw new Exception("Doctor not found");

            if (doctorDto.LicenseNumber != null) existingDoctor.LicenseNumber = doctorDto.LicenseNumber;
            if (doctorDto.Certificate != null) existingDoctor.Certificate = doctorDto.Certificate;
            if (doctorDto.ConsultationFee != null) existingDoctor.ConsultationFee = doctorDto.ConsultationFee.Value;
            if (doctorDto.Address != null) existingDoctor.Address = doctorDto.Address;
            if (doctorDto.GraduationDate != null) existingDoctor.GraduationDate = doctorDto.GraduationDate.Value;
            if (doctorDto.DepartmentId != null) existingDoctor.DepartmentId = doctorDto.DepartmentId.Value;
            if (doctorDto.ImageProfile != null) existingDoctor.ImageProfile = doctorDto.ImageProfile;

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

        public async Task<List<DoctorPendingDto>> GetPendingDoctorsAsync()
        {
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(d => !d.IsVerified, d => d.User);

            return doctors.Select(d => new DoctorPendingDto
            {
                UserId = d.UserId,
                FullName = d.User.FirstName + " " + d.User.LastName,
                PhoneNumber = d.User.PhoneNumber!,
                LicenseImage =d.LicenseImage!,
                CertificateImage = d.CertificateImage!,
                SSNImg = d.SSNImage!,
                ConsultationFee = d.ConsultationFee,
                Address = d.Address,
                DepartmentId = d.DepartmentId,
                IsVerified = d.IsVerified,
            }).ToList();
        }

        public async Task<(bool Success, string Message)> ApproveDoctorAsync(string userId)
        {
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(d => d.UserId == userId, d => d.User);

            var doctor = doctors.FirstOrDefault();

            if (doctor is null)
                return (false, "Doctor not found.");

            if (doctor.IsVerified)
                return (false, "Doctor is already verified.");

            doctor.IsVerified = true;
            _unitOfWork.GetRepository<Doctor>().Update(doctor);

            var existingWallet = (await _unitOfWork.GetRepository<DoctorWallet>()
                .GetAllAsync(w => w.DoctorId == doctor.Id)).FirstOrDefault();

            if (existingWallet is null)
            {
                await _unitOfWork.GetRepository<DoctorWallet>().AddAsync(new DoctorWallet
                {
                    DoctorId = doctor.Id,
                    Balance = 0,
                    PendingBalance = 0
                });
            }

            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendDoctorApprovalAsync(
                toEmail: doctor.User.Email!,
                toName: $"{doctor.User.FirstName} {doctor.User.LastName}");

            return (true, "Doctor approved successfully.");
        }

        // جوه DoctorManager.cs - implementation

        public async Task<DoctorReadDTo?> GetByUserIdAsync(string userId)
        {
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(d => d.UserId == userId, d => d.Department, d => d.User);

            var doctor = doctors.FirstOrDefault();

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
                FirstName = doctor.User?.FirstName,
                LastName = doctor.User?.LastName,
                ImageProfile = doctor.ImageProfile
            };
        }

        public async Task UpdateOwnProfileAsync(string userId, DoctorUpdateDto doctorDto)
        {
            var doctors = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllAsync(d => d.UserId == userId);

            var existingDoctor = doctors.FirstOrDefault();

            if (existingDoctor == null)
                throw new Exception("Doctor profile not found for this user.");

            if (doctorDto.LicenseNumber != null) existingDoctor.LicenseNumber = doctorDto.LicenseNumber;
            if (doctorDto.Certificate != null) existingDoctor.Certificate = doctorDto.Certificate;
            if (doctorDto.ConsultationFee != null) existingDoctor.ConsultationFee = doctorDto.ConsultationFee.Value;
            if (doctorDto.Address != null) existingDoctor.Address = doctorDto.Address;
            if (doctorDto.GraduationDate != null) existingDoctor.GraduationDate = doctorDto.GraduationDate.Value;
            if (doctorDto.DepartmentId != null) existingDoctor.DepartmentId = doctorDto.DepartmentId.Value;
            if (doctorDto.ImageProfile != null) existingDoctor.ImageProfile = doctorDto.ImageProfile;

            _unitOfWork.GetRepository<Doctor>().Update(existingDoctor);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
