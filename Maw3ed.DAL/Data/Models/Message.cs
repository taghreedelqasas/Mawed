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

        public string Content { get; set; }

        public bool IsRead { get; set; }
    }
}
