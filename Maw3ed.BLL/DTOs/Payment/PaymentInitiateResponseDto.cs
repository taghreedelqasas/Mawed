using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.Payment
{
    public class PaymentInitiateResponseDto
    {
        public int PaymentId { get; set; }
        public string IframeUrl { get; set; } = string.Empty;
    }
}
