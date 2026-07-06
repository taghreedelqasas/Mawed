using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
   public class WalletTransaction
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }

        public int? AppointmentId { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Doctor Doctor { get; set; }

        public Appointment Appointment { get; set; }
    }
}
