using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.Payment
{
   public class PaymobRefundResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
