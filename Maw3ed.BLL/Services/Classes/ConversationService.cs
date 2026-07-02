using Maw3ed.BLL.DTOs.ConversationDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Reposatries.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConversationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConversationDto> GetOrCreateConversationAsync(
       int patientId, int doctorId)
        {
            // تحقق إن فيه موعد بين المريض والدكتور
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .FindAsync(a => a.PatientId == patientId
                             && a.DoctorId == doctorId
                             && (a.Status == AppointmentStatus.Confirmed
                                 || a.Status == AppointmentStatus.Completed));

            if (!appointments.Any())
                throw new Exception("مينفعش تبدأ محادثة، لازم يكون عندك موعد مع الدكتور ده الأول");

            // دور على محادثة موجودة
            var conversations = await _unitOfWork.GetRepository<Conversation>()
                .FindAsync(c => c.PatientId == patientId && c.DoctorId == doctorId);

            var existing = conversations.FirstOrDefault();

            if (existing != null)
                return MapToDto(existing);

            // لو مش موجودة عملها جديدة
            var newConversation = new Conversation
            {
                PatientId = patientId,
                DoctorId = doctorId
            };

            await _unitOfWork.GetRepository<Conversation>().AddAsync(newConversation);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(newConversation);
        }
        public async Task<IEnumerable<ConversationDto>> GetPatientConversationsAsync(
            int patientId)
        {
            var conversations = await _unitOfWork.GetRepository<Conversation>()
                .FindAsync(c => c.PatientId == patientId);

            return conversations.Select(c => MapToDto(c));
        }

        public async Task<IEnumerable<ConversationDto>> GetDoctorConversationsAsync(
            int doctorId)
        {
            var conversations = await _unitOfWork.GetRepository<Conversation>()
                .FindAsync(c => c.DoctorId == doctorId);

            return conversations.Select(c => MapToDto(c));
        }

        public async Task<MessageDto> SendMessageAsync(
            int conversationId, string senderUserId, SendMessageDto dto)
        {
            // تأكد إن المحادثة موجودة
            var conversation = await _unitOfWork.GetRepository<Conversation>()
                .GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("المحادثة مش موجودة");

            // ابعت الرسالة
            var message = new Message
            {
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                Content = dto.Content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow  // ← أضيفي السطر ده


            };

            await _unitOfWork.GetRepository<Message>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return MapMessageToDto(message);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId)
        {
            var messages = await _unitOfWork.GetRepository<Message>()
                .FindAsync(m => m.ConversationId == conversationId);

            return messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => MapMessageToDto(m));
        }

        // Helpers
        private ConversationDto MapToDto(Conversation c) => new ConversationDto
        {
            Id = c.Id,
            PatientId = c.PatientId,
            DoctorId = c.DoctorId,
            Messages = c.Messages?.Select(m => MapMessageToDto(m)).ToList()
                       ?? new List<MessageDto>()
        };

        private MessageDto MapMessageToDto(Message m) => new MessageDto
        {
            Id = m.Id,
            SenderUserId = m.SenderUserId,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        };
    }
}
