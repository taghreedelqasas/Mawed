using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Maw3ed.BLL.AI.DTOs
{
    public class BedrockChatResponse
    {
        [JsonPropertyName("output_text")]
        public string OutputText { get; set; } = string.Empty;
    }
}
