using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.Payment
{
    public class InitiatePaymentDto
    {
        public string PaymentMethod { get; set; } // "Card" or "Wallet"
    }
}
