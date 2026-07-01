using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class ChatSession : AuditableEntity
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public Patient Patient { get; set; }

        public ICollection<ChatMessage> Messages { get; set; }
            = new List<ChatMessage>();
    }
}
