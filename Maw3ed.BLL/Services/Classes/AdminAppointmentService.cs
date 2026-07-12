using Maw3ed.BLL.DTOs.AdminDashboard;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{
    public class AdminAppointmentService : IAdminAppointmentService
    {
        private readonly AppDbContext _context;

        private static readonly (AppointmentStatus Status, string Label)[] StatusLabels =
        {
            (AppointmentStatus.Pending,   "قيد الانتظار"),
            (AppointmentStatus.Confirmed, "مؤكد"),
            (AppointmentStatus.Completed, "مكتمل"),
            (AppointmentStatus.Cancelled, "ملغي"),
        };

        public AdminAppointmentService(AppDbContext context)
        {
            _context = context;
        }

        // ── قائمة المواعيد بالـ Pagination + فلاتر اختيارية ──────────
        public async Task<AdminAppointmentsPagedResultDto> GetAppointmentsAsync(
            int page, int pageSize, string? status, DateTime? date, string? search)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 6;

            var query = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.DoctorAvailability)
                .Include(a => a.Payment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            if (date.HasValue)
            {
                var day = date.Value.Date;
                var nextDay = day.AddDays(1);
                query = query.Where(a => a.DoctorAvailability.StartTime >= day
                                       && a.DoctorAvailability.StartTime < nextDay);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(a =>
                    (a.Doctor.User.FirstName + " " + a.Doctor.User.LastName).Contains(term) ||
                    (a.Patient.User.FirstName + " " + a.Patient.User.LastName).Contains(term));
            }

            var totalCount = await query.CountAsync();

            var raw = await query
                .OrderByDescending(a => a.DoctorAvailability.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    DoctorName = a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                    PatientName = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
                    Fee = a.Payment != null ? a.Payment.Amount : a.Doctor.ConsultationFee,
                    a.DoctorAvailability.StartTime,
                    a.Status
                })
                .ToListAsync();

            var items = raw.Select(a => new AdminAppointmentDto
            {
                Id = a.Id,
                BookingNumber = $"A-{1000 + a.Id}",
                DoctorName = "د. " + a.DoctorName,
                PatientName = a.PatientName,
                Fee = a.Fee,
                Date = a.StartTime.ToString("yyyy-MM-dd"),
                Time = FormatArabicTime(a.StartTime),
                Status = a.Status.ToString(),
                StatusLabel = StatusLabels.FirstOrDefault(s => s.Status == a.Status).Label ?? a.Status.ToString()
            }).ToList();

            return new AdminAppointmentsPagedResultDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── كروت الـ KPI فوق الجدول ────────────────────────────────
        public async Task<AdminAppointmentsSummaryDto> GetSummaryAsync()
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            var todayBookings = await _context.Appointments
                .CountAsync(a => a.DoctorAvailability.StartTime >= todayStart
                              && a.DoctorAvailability.StartTime < todayEnd);

            var statusCounts = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int CountOf(AppointmentStatus s) => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

            return new AdminAppointmentsSummaryDto
            {
                TodayBookings = todayBookings,
                Pending = CountOf(AppointmentStatus.Pending),
                Confirmed = CountOf(AppointmentStatus.Confirmed),
                Completed = CountOf(AppointmentStatus.Completed),
                Cancelled = CountOf(AppointmentStatus.Cancelled)
            };
        }

        private static string FormatArabicTime(DateTime dt)
        {
            var hour12 = dt.Hour % 12 == 0 ? 12 : dt.Hour % 12;
            var period = dt.Hour < 12 ? "ص" : "م";
            return $"{hour12:00}:{dt.Minute:00} {period}";
        }
    }
}
