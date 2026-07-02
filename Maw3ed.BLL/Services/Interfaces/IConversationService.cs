using Maw3ed.BLL.DTOs.ConversationDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IConversationService
    {
        // ابدأ محادثة جديدة أو جيب الموجودة
        Task<ConversationDto> GetOrCreateConversationAsync(int patientId, int doctorId);

        // جيب كل محادثات المريض
        Task<IEnumerable<ConversationDto>> GetPatientConversationsAsync(int patientId);

        // جيب كل محادثات الدكتور
        Task<IEnumerable<ConversationDto>> GetDoctorConversationsAsync(int doctorId);

        // بعت رسالة
        Task<MessageDto> SendMessageAsync(int conversationId, string senderUserId, SendMessageDto dto);

        // جيب رسايل المحادثة
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId);
    }
}
