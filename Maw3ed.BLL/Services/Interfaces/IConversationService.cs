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
        Task<ConversationDto> GetOrCreateConversationAsync(int patientId, int doctorId);
        Task<IEnumerable<ConversationDto>> GetPatientConversationsAsync(int patientId);
        Task<IEnumerable<ConversationDto>> GetDoctorConversationsAsync(int doctorId);

        Task<MessageDto> SendMessageAsync(int conversationId, string senderUserId, SendMessageDto dto);

        Task<MessageDto> SendAttachmentAsync(
            int conversationId, string senderUserId,
            string attachmentUrl, string attachmentName, string attachmentType, string? caption);

        Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string userId);

        Task MarkMessagesAsReadAsync(int conversationId, string userId);
    }


}
