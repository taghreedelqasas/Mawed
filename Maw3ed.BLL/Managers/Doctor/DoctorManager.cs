using Maw3ed.DAL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;



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

        public async Task<List<DoctorPendingDto>> GetPendingDoctorsAsync()
        {
            var doctors = await _unitOfWork
               .GetRepository<Maw3ed.DAL.Doctor>()
               .GetAllWithIncludes()
               .Where(d => !d.IsVerified)
               .Include(d => d.User)        // navigation to ApplicationUser
               .Select(d => new DoctorPendingDto
               {
                   UserId = d.UserId,
                   FullName = d.User.FirstName + " " + d.User.LastName,
                   Email = d.User.Email!,
                   PhoneNumber = d.User.PhoneNumber!,
                   LicenseNumber = d.LicenseNumber,
                   Certificate = d.Certificate,
                   ConsultationFee = d.ConsultationFee,
                   Address = d.Address,
                   GraduationDate = d.GraduationDate,
                   DepartmentId = d.DepartmentId,
                   RegisteredAt = d.CreatedAt
               })
               .ToListAsync();

            return doctors;
        }

        public async Task<(bool Success, string Message)> ApproveDoctorAsync(string userId)
        {
            var doctor = await _unitOfWork
                .GetRepository<Doctor>()
                .GetAllWithIncludes()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor is null)
                return (false, "Doctor not found.");

            if (doctor.IsVerified)
                return (false, "Doctor is already verified.");

            doctor.IsVerified = true;
            _unitOfWork.GetRepository<Doctor>().Update(doctor);
            _unitOfWork.SaveChanges();

            await _emailService.SendDoctorApprovalAsync(
                toEmail: doctor.User.Email!,
                toName: $"{doctor.User.FirstName} {doctor.User.LastName}");

            return (true, "Doctor approved successfully.");
        }


    }
}
