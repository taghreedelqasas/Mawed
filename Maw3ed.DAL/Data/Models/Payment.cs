
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class Payment : AuditableEntity
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public decimal SystemFee { get; set; }

        public PaymentStatus Status { get; set; }

        public PaymentMethod Method { get; set; }

        public int AppointmentId { get; set; }

        public Appointment Appointment { get; set; }
        public string? PaymobTransactionId { get; set; }   // بنخزنه وقت نجاح الدفع، عشان نقدر نعمل عليه Refund بعدين
        public DateTime? RefundedAt { get; set; }
    }
}
