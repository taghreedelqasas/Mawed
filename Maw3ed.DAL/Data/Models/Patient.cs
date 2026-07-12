using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class Patient : AuditableEntity
    {
        public int Id { get; set; }

        public string? MedicalHistory { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();

        public ICollection<Conversation> Conversations { get; set; }
        = new List<Conversation>();

        public ICollection<ChatSession> ChatSessions { get; set; }
            = new List<ChatSession>();

        public ICollection<MedicalFile> MedicalFiles { get; set; }
        = new List<MedicalFile>();

        public ICollection<MedicalReportAnalysis> MedicalReportAnalyses { get; set; }
    = new List<MedicalReportAnalysis>();
    }
}
