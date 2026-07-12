using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class Message : AuditableEntity
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public string SenderUserId { get; set; }
        public string? Content { get; set; }
        public bool IsRead { get; set; }

        // دعم المرفقات (نتائج تحاليل / تقارير)
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachmentType { get; set; } // "pdf" أو "image"
    }
}
