using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.AI.DTOs
{
    public class ChatHistoryItem
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsAiResponse { get; set; }
        public DateTime SentAt { get; set; }
    }
}
