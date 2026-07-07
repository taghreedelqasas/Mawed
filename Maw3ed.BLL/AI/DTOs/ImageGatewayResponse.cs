using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Maw3ed.BLL.AI.DTOs
{
    public class ImageGatewayResponse
    {
        [JsonPropertyName("output_text")]
        public string OutputText { get; set; } = string.Empty;
    }
}
