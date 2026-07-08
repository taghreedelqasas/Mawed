using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
    public class WithdrawRequest
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; }

        public string AccountNumber { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Doctor Doctor { get; set; }
    }
}
