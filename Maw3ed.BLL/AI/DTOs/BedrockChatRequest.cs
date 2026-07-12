using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.AI.DTOs
{
    public class BedrockChatRequest
    {
        public string model_id { get; set; } = "";
        public List<BedrockMessage> messages { get; set; } = new();
        public string system_prompt { get; set; } = "";
        public int max_tokens { get; set; } = 300;
    }

    public class BedrockMessage
    {
        public string role { get; set; } = "";
        public string content { get; set; } = "";
    }
}
