using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.ConversationDTOs
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public List<MessageDto> Messages { get; set; }
        public string? PatientImage { get; set; }

        public string? DoctorName { get; set; }
        public string? DoctorImage { get; set; }
        public string? PatientName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
