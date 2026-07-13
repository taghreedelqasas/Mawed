using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
    public class Notification : AuditableEntity
    {
        public int Id { get; set; }

        // UserId لأن الـ Email موجود في ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public int? RelatedEntityId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? SentAtUtc { get; set; }
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed
    }
}
