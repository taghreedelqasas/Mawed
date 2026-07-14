using Maw3ed.BLL.DTOs.AdminPayments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IAdminPaymentsService
    {
        // كروت الملخص الأربعة فوق شاشة "المدفوعات والعمولات"
        Task<PaymentsSummaryDto> GetSummaryAsync();

        // قراءة نسبة العمولة الحالية
        Task<CommissionRateDto> GetCommissionRateAsync();

        // تعديل نسبة العمولة (0 - 100)
        Task<(bool Success, string Message, CommissionRateDto? Data)> UpdateCommissionRateAsync(
            decimal newRate, string? adminUserId);

        // رسم "تحليل الإيرادات والعمولات" آخر N شهر
        Task<RevenueCommissionTrendDto> GetRevenueCommissionTrendAsync(int months);

        // قائمة أحدث المعاملات المالية (مع صفحات وفلترة اختيارية بالحالة)
        Task<AdminTransactionsPagedResultDto> GetTransactionsAsync(int page, int pageSize, string? status);
    }
}
