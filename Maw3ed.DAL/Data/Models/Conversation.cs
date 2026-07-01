using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class Conversation : AuditableEntity
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}
