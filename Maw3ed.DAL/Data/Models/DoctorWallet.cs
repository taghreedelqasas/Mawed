using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Data.Models
{
 
        public class DoctorWallet
        {
            public int Id { get; set; }

            public int DoctorId { get; set; }

            public decimal Balance { get; set; }

            public decimal PendingBalance { get; set; }

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

            public Doctor Doctor { get; set; }
        }
    }

