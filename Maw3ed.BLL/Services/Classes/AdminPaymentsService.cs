using Maw3ed.BLL.DTOs.AdminDashboard;
using Maw3ed.BLL.DTOs.AdminPayments;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{
    public class AdminPaymentsService : IAdminPaymentsService
    {
        private readonly AppDbContext _context;

        private static readonly string[] ArabicMonths =
        {
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        // خريطة تحويل حالة الدفع الفعلية (PaymentStatus) للتسمية العربية
        // اللي متوقعة في تصميم شاشة "المدفوعات والعمولات"
        private static readonly Dictionary<PaymentStatus, string> StatusLabels = new()
        {
            [PaymentStatus.Paid] = "معتمد",
            [PaymentStatus.Pending] = "بانتظار المراجعة",
            [PaymentStatus.Failed] = "مرفوض",
            [PaymentStatus.Refunded] = "مسترد"
        };

        public AdminPaymentsService(AppDbContext context)
        {
            _context = context;
        }

        // ── كروت الملخص الأربعة ───────────────────────────────────────
        public async Task<PaymentsSummaryDto> GetSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            // إجمالي الإيرادات (مدفوعات ناجحة فقط) - تراكمي، ومقارنة بتراكمي حتى بداية الشهر الحالي
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var totalRevenueLastMonth = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt < monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // عمولة المنصة (من المدفوعات الناجحة)
            var totalCommission = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.SystemFee) ?? 0;
            var totalCommissionLastMonth = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt < monthStart)
                .SumAsync(p => (decimal?)p.SystemFee) ?? 0;

            // أرباح الأطباء = الإيرادات - العمولة (من المدفوعات الناجحة)
            var doctorsProfit = totalRevenue - totalCommission;
            var doctorsProfitLastMonth = totalRevenueLastMonth - totalCommissionLastMonth;

            // المدفوعات المعلقة: مدفوعات لسه Pending (ماتحصّلتش)
            var pendingPayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var pendingPaymentsLastMonth = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var commissionRate = await GetOrCreateSettingsAsync();

            return new PaymentsSummaryDto
            {
                PendingPayments = BuildCard(pendingPayments, pendingPaymentsLastMonth),
                DoctorsProfit = BuildCard(doctorsProfit, doctorsProfitLastMonth),
                PlatformCommission = BuildCard(totalCommission, totalCommissionLastMonth),
                TotalRevenue = BuildCard(totalRevenue, totalRevenueLastMonth),
                CommissionRate = commissionRate.CommissionRate
            };
        }

        // ── نسبة العمولة: قراءة ──────────────────────────────────────
        public async Task<CommissionRateDto> GetCommissionRateAsync()
        {
            var settings = await GetOrCreateSettingsAsync();

            return new CommissionRateDto
            {
                CommissionRate = settings.CommissionRate,
                UpdatedAt = settings.UpdatedAt,
                UpdatedBy = settings.UpdatedBy
            };
        }

        // ── نسبة العمولة: تعديل ──────────────────────────────────────
        public async Task<(bool Success, string Message, CommissionRateDto? Data)> UpdateCommissionRateAsync(
            decimal newRate, string? adminUserId)
        {
            if (newRate < 0 || newRate > 100)
                return (false, "نسبة العمولة يجب أن تكون بين 0 و100.", null);

            var settings = await GetOrCreateSettingsAsync();

            settings.CommissionRate = newRate;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = adminUserId;

            await _context.SaveChangesAsync();

            return (true, "تم تعديل نسبة العمولة بنجاح.", new CommissionRateDto
            {
                CommissionRate = settings.CommissionRate,
                UpdatedAt = settings.UpdatedAt,
                UpdatedBy = settings.UpdatedBy
            });
        }

        // ── رسم "تحليل الإيرادات والعمولات" ───────────────────────────
        public async Task<RevenueCommissionTrendDto> GetRevenueCommissionTrendAsync(int months)
        {
            if (months <= 0) months = 6;
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));

            var payments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= start)
                .Select(p => new { p.CreatedAt, p.Amount, p.SystemFee })
                .ToListAsync();

            var points = new List<RevenueCommissionPointDto>();
            for (int i = 0; i < months; i++)
            {
                var monthDate = start.AddMonths(i);
                var monthPayments = payments
                    .Where(p => p.CreatedAt.Year == monthDate.Year && p.CreatedAt.Month == monthDate.Month)
                    .ToList();

                points.Add(new RevenueCommissionPointDto
                {
                    Year = monthDate.Year,
                    Month = monthDate.Month,
                    Label = ArabicMonths[monthDate.Month - 1],
                    Revenue = monthPayments.Sum(p => p.Amount),
                    Commission = monthPayments.Sum(p => p.SystemFee)
                });
            }

            return new RevenueCommissionTrendDto { Points = points };
        }

        // ── قائمة أحدث المعاملات المالية ───────────────────────────────
        public async Task<AdminTransactionsPagedResultDto> GetTransactionsAsync(int page, int pageSize, string? status)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Payments
                .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Appointment).ThenInclude(a => a.Patient).ThenInclude(pa => pa.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<PaymentStatus>(status, true, out var statusFilter))
            {
                query = query.Where(p => p.Status == statusFilter);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AdminTransactionDto
                {
                    Id = p.Id,
                    TransactionCode = $"TX-{1000 + p.Id}",
                    DoctorName = p.Appointment.Doctor.User.FirstName + " " + p.Appointment.Doctor.User.LastName,
                    PatientName = p.Appointment.Patient.User.FirstName + " " + p.Appointment.Patient.User.LastName,
                    ConsultationFee = p.Amount,
                    Commission = p.SystemFee,
                    DoctorNet = p.Amount - p.SystemFee,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

// تعريب اسم الحالة بعد الجلب (مش قابلة للترجمة داخل استعلام EF)
            foreach (var item in items)
            {
                if (Enum.TryParse<PaymentStatus>(item.Status, out var parsed) && StatusLabels.TryGetValue(parsed, out var label))
                    item.StatusLabel = label;
            }

            return new AdminTransactionsPagedResultDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── Helper: يجيب صف الإعدادات، ولو مش موجود يعمل واحد بالقيمة الافتراضية ─
        private async Task<PlatformSetting> GetOrCreateSettingsAsync()
        {
            var settings = await _context.PlatformSettings.FirstOrDefaultAsync();
            if (settings is null)
            {
                settings = new PlatformSetting
                {
                    CommissionRate = 10m,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PlatformSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        // ── Helper: بناء كارت الـ KPI ونسبة التغيير ──────────────────
        private static KpiCardDto BuildCard(decimal current, decimal previous)
        {
            decimal change = previous == 0
                ? (current == 0 ? 0 : 100)
                : Math.Round((current - previous) / previous * 100, 1);

            return new KpiCardDto
            {
                Value = current,
                ChangePercentage = change,
                ComparisonLabel = "الشهر الماضي",
                ComparisonValue = previous
            };
        }
    }
}