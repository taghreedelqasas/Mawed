using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorManager
{
    public class DoctorService : IDoctorService
    {
        private const string BaseUrl = "https://mawed.runasp.net";
        private readonly AppDbContext _context;

        public DoctorService(AppDbContext context)
        {
            _context = context;
        }

        private static string? ToFullUrl(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path.StartsWith("http")) return path;
            return $"{BaseUrl}{path}";
        }

        public async Task<IEnumerable<DoctorSearchResultDto>> SearchDoctorsAsync(
            DoctorSearchFilterDto filter)
        {
            var today = DateTime.UtcNow.Date;

            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .Include(d => d.Reviews)
                .Include(d => d.Availabilities)
                .Where(d => d.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.DoctorName))
                query = query.Where(d =>
                    (d.User.FirstName + " " + d.User.LastName)
                    .Contains(filter.DoctorName));

            if (!string.IsNullOrEmpty(filter.Specialty))
                query = query.Where(d =>
                    d.Department.Name.Contains(filter.Specialty));

            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(d =>
                    d.Address.Contains(filter.Location));

            if (filter.Date.HasValue)
            {
                var filterDate = filter.Date.Value.Date;
                query = query.Where(d =>
                    d.Availabilities.Any(a =>
                        a.StartTime.Date == filterDate && !a.IsBooked));
            }

            var doctors = await query.Select(d => new DoctorSearchResultDto
            {
                Id = d.Id,
                FullName = d.User.FirstName + " " + d.User.LastName,
                Specialty = d.Department.Name,
                Location = d.Address,
                ImageProfile = ToFullUrl(d.ImageProfile),
                AverageRating = d.Reviews.Any()
                    ? Math.Round(d.Reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews = d.Reviews.Count,
                ConsultationFee = d.ConsultationFee,
                IsVerified = d.IsVerified,
                IsAvailableToday = d.Availabilities
                    .Any(a => a.StartTime.Date == today && !a.IsBooked)
            }).ToListAsync();

            // Sort
            doctors = filter.SortBy switch
            {
                "rating" => doctors.OrderByDescending(d => d.AverageRating).ToList(),
                "price" => doctors.OrderBy(d => d.ConsultationFee).ToList(),
                "available" => doctors.OrderByDescending(d => d.IsAvailableToday).ToList(),
                _ => doctors.OrderByDescending(d => d.AverageRating).ToList()
            };

            return doctors;
        }

        public async Task<DoctorProfileDto?> GetDoctorProfileAsync(int doctorId)
        {
            var today = DateTime.UtcNow.Date;

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .Include(d => d.Reviews)
                .Include(d => d.Availabilities)
                .Include(d => d.Conversations)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsVerified);

            if (doctor == null) return null;

            var totalPatients = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            return new DoctorProfileDto
            {
                Id = doctor.Id,
                FullName = doctor.User.FirstName + " " + doctor.User.LastName,
                Specialty = doctor.Department.Name,
                Location = doctor.Address,
                ImageProfile = ToFullUrl(doctor.ImageProfile),
                AverageRating = doctor.Reviews.Any()
                    ? Math.Round(doctor.Reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews = doctor.Reviews.Count,
                TotalPatients = totalPatients,
                ConsultationFee = doctor.ConsultationFee,
                Certificate = doctor.Certificate,
                YearsOfExperience = DateTime.UtcNow.Year - doctor.GraduationDate.Year,
                IsVerified = doctor.IsVerified,
                IsAvailableToday = doctor.Availabilities
                    .Any(a => a.StartTime.Date == today && !a.IsBooked),
                TodaySlots = doctor.Availabilities
                    .Where(a => a.StartTime.Date == today && !a.IsBooked)
                    .OrderBy(a => a.StartTime)
                    .Select(a => new AvailableSlotDto
                    {
                        Id = a.Id,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime
                    }).ToList()
            };
        }
    }
}
