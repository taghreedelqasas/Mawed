using Maw3ed.BLL.DTOs.AdminDashboard;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;

using Maw3ed.BLL.DTOs.AdminDashboard;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{

    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        private static readonly string[] ArabicMonths =
        {
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        // ── نظرة عامة: الكروت الـ 8 ───────────────────────────────────
        public async Task<DashboardOverviewDto> GetOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var todayEnd = todayStart.AddDays(1);
            var yesterdayStart = todayStart.AddDays(-1);

            var monthStart = new DateTime(now.Year, now.Month, 1);
            var prevMonthStart = monthStart.AddMonths(-1);

            // مواعيد اليوم (بتاريخ الميعاد نفسه) مقارنة بأمس
            var todayCount = await _context.Appointments
                .CountAsync(a => a.DoctorAvailability.StartTime >= todayStart
                              && a.DoctorAvailability.StartTime < todayEnd);
            var yesterdayCount = await _context.Appointments
                .CountAsync(a => a.DoctorAvailability.StartTime >= yesterdayStart
                              && a.DoctorAvailability.StartTime < todayStart);

            // إجمالي المواعيد (تراكمي) مقارنة بإجمالي حتى نهاية الشهر الماضي
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalAppointmentsLastMonth = await _context.Appointments
                .CountAsync(a => a.CreatedAt < monthStart);

            // إجمالي المرضى (تراكمي)
            var totalPatients = await _context.Patients.CountAsync();
            var totalPatientsLastMonth = await _context.Patients
                .CountAsync(p => p.CreatedAt < monthStart);

            // إجمالي الأطباء الموثقين (تراكمي)
            var totalDoctors = await _context.Doctors.CountAsync(d => d.IsVerified);
            var totalDoctorsLastMonth = await _context.Doctors
                .CountAsync(d => d.IsVerified && d.CreatedAt < monthStart);

            // معدل الإتمام: نسبة "Completed" لمواعيد الشهر الحالي مقابل الشهر الماضي
            var thisMonthTotal = await _context.Appointments
                .CountAsync(a => a.CreatedAt >= monthStart);
            var thisMonthCompleted = await _context.Appointments
                .CountAsync(a => a.CreatedAt >= monthStart && a.Status == AppointmentStatus.Completed);

            var lastMonthTotal = await _context.Appointments
                .CountAsync(a => a.CreatedAt >= prevMonthStart && a.CreatedAt < monthStart);
            var lastMonthCompleted = await _context.Appointments
                .CountAsync(a => a.CreatedAt >= prevMonthStart && a.CreatedAt < monthStart
                              && a.Status == AppointmentStatus.Completed);

            var completionRate = thisMonthTotal == 0
                ? 0 : Math.Round((decimal)thisMonthCompleted / thisMonthTotal * 100, 1);
            var lastCompletionRate = lastMonthTotal == 0
                ? 0 : Math.Round((decimal)lastMonthCompleted / lastMonthTotal * 100, 1);

            // إجمالي الإيرادات وعمولة المنصة (من المدفوعات الناجحة فقط - تراكمي)
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var totalRevenueLastMonth = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt < monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

var totalCommission = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.SystemFee) ?? 0;
            var totalCommissionLastMonth = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt < monthStart)
                .SumAsync(p => (decimal?)p.SystemFee) ?? 0;

            // الاستشارات النشطة: عدد المحادثات اللي فيها رسالة خلال آخر 30 يوم
            var last30Days = now.AddDays(-30);
            var prev30Days = last30Days.AddDays(-30);

            var activeConsultations = await _context.Set<Message>()
                .Where(m => m.CreatedAt >= last30Days)
                .Select(m => m.ConversationId)
                .Distinct()
                .CountAsync();

            var activeConsultationsPrev = await _context.Set<Message>()
                .Where(m => m.CreatedAt >= prev30Days && m.CreatedAt < last30Days)
                .Select(m => m.ConversationId)
                .Distinct()
                .CountAsync();

            return new DashboardOverviewDto
            {
                TodayAppointments = BuildCard(todayCount, yesterdayCount, "أمس"),
                TotalAppointments = BuildCard(totalAppointments, totalAppointmentsLastMonth, "الشهر الماضي"),
                TotalPatients = BuildCard(totalPatients, totalPatientsLastMonth, "الشهر الماضي"),
                TotalDoctors = BuildCard(totalDoctors, totalDoctorsLastMonth, "الشهر الماضي"),
                CompletionRate = BuildCard(completionRate, lastCompletionRate, "الشهر الماضي"),
                PlatformCommission = BuildCard(totalCommission, totalCommissionLastMonth, "الشهر الماضي"),
                TotalRevenue = BuildCard(totalRevenue, totalRevenueLastMonth, "الشهر الماضي"),
                ActiveConsultations = BuildCard(activeConsultations, activeConsultationsPrev, "الشهر الماضي")
            };
        }

        // ── توزيع حالات المواعيد (الدونات) لشهر معين ─────────────────
        public async Task<AppointmentStatusDistributionDto> GetAppointmentStatusDistributionAsync(int? year, int? month)
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var rangeStart = new DateTime(y, m, 1);
            var rangeEnd = rangeStart.AddMonths(1);

            var counts = await _context.Appointments
                .Where(a => a.CreatedAt >= rangeStart && a.CreatedAt < rangeEnd)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = counts.Sum(c => c.Count);

            // ملحوظة: الـ Enum الحالي AppointmentStatus مفيهوش حالة "لم يحضر/NoShow"
            // زي اللي في تصميم الداشبورد، فلو محتاجينها لازم تتضاف كحالة جديدة في الـ Enum.
            var labels = new (AppointmentStatus Status, string Label)[]
            {
                (AppointmentStatus.Completed, "مكتمل"),
                (AppointmentStatus.Pending,   "قيد الانتظار"),
                (AppointmentStatus.Confirmed, "مؤكد"),
                (AppointmentStatus.Cancelled, "ملغي"),
            };

            var slices = labels.Select(l =>
            {
                var count = counts.FirstOrDefault(c => c.Status == l.Status)?.Count ?? 0;
                return new StatusSliceDto
                {
                    Status = l.Status.ToString(),
                    Label = l.Label,
                    Count = count,
                    Percentage = total == 0 ? 0 : Math.Round((decimal)count / total * 100, 1)
                };
            }).ToList();

            var completedPct = slices.FirstOrDefault(s => s.Status == nameof(AppointmentStatus.Completed))?.Percentage ?? 0;

            return new AppointmentStatusDistributionDto
            {
                Year = y,
                Month = m,
                TotalAppointments = total,
                CompletedPercentage = completedPct,
                Slices = slices
            };
        }

        // ── اتجاه المواعيد/الإيرادات آخر N شهر ───────────────────────
        public async Task<MonthlyTrendDto> GetMonthlyTrendAsync(string type, int months)
        {
            if (months <= 0) months = 12;
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));

            var isRevenue = string.Equals(type, "revenue", StringComparison.OrdinalIgnoreCase);
            var points = new List<MonthlyTrendPointDto>();

            if (isRevenue)
            {
                var payments = await _context.Payments
                    .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= start)
                    .Select(p => new { p.CreatedAt, p.Amount })
                    .ToListAsync();

                for (int i = 0; i < months; i++)
                {
                    var monthDate = start.AddMonths(i);
                    var value = payments
                        .Where(p => p.CreatedAt.Year == monthDate.Year && p.CreatedAt.Month == monthDate.Month)
                        .Sum(p => p.Amount);

                    points.Add(new MonthlyTrendPointDto
                    {
                        Year = monthDate.Year,
                        Month = monthDate.Month,
                        Label = ArabicMonths[monthDate.Month - 1],
                        Value = value
                    });
                }

                return new MonthlyTrendDto { Type = "revenue", Points = points };
            }

            var appointmentDates = await _context.Appointments
                .Where(a => a.CreatedAt >= start)
                .Select(a => a.CreatedAt)
                .ToListAsync();

            for (int i = 0; i < months; i++)
            {
                var monthDate = start.AddMonths(i);
                var value = appointmentDates
                    .Count(d => d.Year == monthDate.Year && d.Month == monthDate.Month);

                points.Add(new MonthlyTrendPointDto
                {
                    Year = monthDate.Year,
                    Month = monthDate.Month,
                    Label = ArabicMonths[monthDate.Month - 1],
                    Value = value
                });
            }

            return new MonthlyTrendDto { Type = "appointments", Points = points };
        }
        public async Task<IEnumerable<AdminPatientDto>> GetAllPatientsAsync()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .Select(p => new AdminPatientDto
                {
                    Id = p.Id,
                    FullName = p.User.FirstName + " " + p.User.LastName,
                    Email = p.User.Email!,
                    ProfilePictureUrl = p.User.ProfilePictureUrl,
                    TotalAppointments = p.Appointments.Count
                })
                .ToListAsync();

            return patients;
        }

        public async Task<IEnumerable<AdminDoctorDto>> GetAllDoctorsAsync()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .Include(d => d.Reviews)
                .Include(d => d.Availabilities)
                    .ThenInclude(a => a.Appointment)
                .Select(d => new AdminDoctorDto
                {
                    Id = d.Id,
                    FullName = d.User.FirstName + " " + d.User.LastName,
                    Department = d.Department.Name,
                    Address = d.Address,
                    ProfilePictureUrl = d.ImageProfile,
                    AverageRating = d.Reviews.Any()
                        ? Math.Round(d.Reviews.Average(r => r.Rating), 1)
                        : 0,
                    TotalReviews = d.Reviews.Count,
                    TotalAppointments = d.Availabilities
                        .Count(a => a.Appointment != null)
                })
                .ToListAsync();

            return doctors;
        }

        // ── جديد: بروفايل مريض كامل لـ "عرض الملف الشخصي" ─────────────
        public async Task<AdminPatientDetailDto?> GetPatientDetailAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .Include(p => p.MedicalFiles)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return null;

            return new AdminPatientDetailDto
            {
                Id = patient.Id,
                FullName = patient.User.FirstName + " " + patient.User.LastName,
                Email = patient.User.Email!,
                PhoneNumber = patient.User.PhoneNumber,
                ProfilePictureUrl = patient.User.ProfilePictureUrl,
                Gender = FormatGender(patient.User.Gender),
                Age = CalculateAge(patient.User.BirthDate),
                RegisteredAt = patient.CreatedAt.ToString("yyyy-MM-dd"),
                IsActive = patient.User.IsActive,
                MedicalHistory = patient.MedicalHistory,
                MedicalFiles = patient.MedicalFiles
                    .OrderByDescending(f => f.UploadedAtUtc)
                    .Select(f => new AdminMedicalFileDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        FileUrl = f.FileUrl,
                        FileType = f.FileType,
                        CategoryLabel = MedicalFileCategoryLabels
                            .FirstOrDefault(c => c.Category == f.Category).Label ?? f.Category.ToString(),
                        UploadedAt = f.UploadedAtUtc.ToString("yyyy-MM-dd")
                    })
                    .ToList()
            };
        }

        // ── جديد: بروفايل طبيب كامل لـ "عرض الملف الشخصي" ─────────────
        public async Task<AdminDoctorDetailDto?> GetDoctorDetailAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .Include(d => d.Reviews)
                .Include(d => d.Availabilities)
                    .ThenInclude(a => a.Appointment)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null) return null;

            return new AdminDoctorDetailDto
            {
                Id = doctor.Id,
                FullName = doctor.User.FirstName + " " + doctor.User.LastName,
                Email = doctor.User.Email!,
                PhoneNumber = doctor.User.PhoneNumber,
                ProfilePictureUrl = doctor.ImageProfile,
                Gender = FormatGender(doctor.User.Gender),
                Age = CalculateAge(doctor.User.BirthDate),
                RegisteredAt = doctor.CreatedAt.ToString("yyyy-MM-dd"),
                IsActive = doctor.User.IsActive,
                Department = doctor.Department.Name,
                Address = doctor.Address,
                LicenseNumber = doctor.LicenseNumber,
                ConsultationFee = doctor.ConsultationFee,
                GraduationDate = doctor.GraduationDate.ToString("yyyy-MM-dd"),
                IsVerified = doctor.IsVerified,
                AverageRating = doctor.Reviews.Any() ? Math.Round(doctor.Reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews = doctor.Reviews.Count,
                TotalAppointments = doctor.Availabilities.Count(a => a.Appointment != null)
            };
        }

        // ── Helper: تنسيق الجنس كنص عربي ──────────────────────────────
        private static string FormatGender(Gender? gender)
        {
            return gender switch
            {
                Gender.Male => "ذكر",
                Gender.Female => "أنثى",
                _ => "غير محدد"
            };
        }

        // ── Helper: حساب العمر من تاريخ الميلاد ───────────────────────
        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        // ── Helper: تسميات فئات الملفات الطبية بالعربي ────────────────
        private static readonly (MedicalFileCategory Category, string Label)[] MedicalFileCategoryLabels =
        {
            (MedicalFileCategory.LabResult, "تحاليل طبية"),
            (MedicalFileCategory.Scan, "أشعة"),
            (MedicalFileCategory.Prescription, "وصفات طبية"),
            (MedicalFileCategory.MedicalReport, "تقارير طبية"),
        };

        // ── Helper: بناء كارت الـ KPI ونسبة التغيير ──────────────────
        private static KpiCardDto BuildCard(decimal current, decimal previous, string comparisonLabel)
        {
            decimal change = previous == 0
                ? (current == 0 ? 0 : 100)
                : Math.Round((current - previous) / previous * 100, 1);

            return new KpiCardDto
            {
                Value = current,
                ChangePercentage = change,
                ComparisonLabel = comparisonLabel,
                ComparisonValue = previous
            };
        }
    }
}