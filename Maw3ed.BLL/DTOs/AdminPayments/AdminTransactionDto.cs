using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.AdminPayments
{
    public class AdminTransactionDto
    {
        public int Id { get; set; }

        // رقم العملية المعروض للأدمن، مبني على رقم الـ Payment (مثال: TX-1023)
        public string TransactionCode { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        // رسوم الكشف (السعر الكامل قبل خصم العمولة)
        public decimal ConsultationFee { get; set; }

        // عمولة المنصة من العملية دي
        public decimal Commission { get; set; }

        // صافي الطبيب (رسوم الكشف - العمولة)
        public decimal DoctorNet { get; set; }

        // حالة الدفع الفعلية زي ما هي مخزنة (Pending / Paid / Failed / Refunded)
        public string Status { get; set; } = string.Empty;

        // نفس الحالة لكن بالتسمية العربية اللي في التصميم (بانتظار المراجعة / معتمد / مرفوض / مسترد)
        public string StatusLabel { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class AdminTransactionsPagedResultDto
    {
        public List<AdminTransactionDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
