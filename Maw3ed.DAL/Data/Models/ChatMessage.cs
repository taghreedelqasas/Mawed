using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class ChatMessage : AuditableEntity
    {
        public int Id { get; set; }

        public int ChatSessionId { get; set; }

        public ChatSession ChatSession { get; set; }

        public string Sender
        {
            get; set;
        }

        public string Message
        {
            get; set;
        }

        public bool IsAiResponse
        {
            get; set;
        }
    }
}
