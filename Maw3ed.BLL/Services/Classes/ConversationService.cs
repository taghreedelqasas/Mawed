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

        public async Task<ConversationDto> GetOrCreateConversationAsync(int patientId, int doctorId)
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .FindAsync(a => a.PatientId == patientId
                             && a.DoctorId == doctorId
                             && (a.Status == AppointmentStatus.Confirmed
                                 || a.Status == AppointmentStatus.Completed));

            if (!appointments.Any())
                throw new Exception("مينفعش تبدأ محادثة، لازم يكون عندك موعد مع الدكتور ده الأول");

            var conversations = await _unitOfWork.GetRepository<Conversation>()
                .FindAsync(c => c.PatientId == patientId && c.DoctorId == doctorId);

            var existing = conversations.FirstOrDefault();
            if (existing != null)
                return MapToDto(existing);

            var newConversation = new Conversation
            {
                PatientId = patientId,
                DoctorId = doctorId
            };

            await _unitOfWork.GetRepository<Conversation>().AddAsync(newConversation);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(newConversation);
        }

        public async Task<IEnumerable<ConversationDto>> GetPatientConversationsAsync(int patientId)
        {
            var conversations = await _unitOfWork.GetRepository<Conversation>()
                .FindAsync(c => c.PatientId == patientId);

            return conversations.Select(c => MapToDto(c));
        }

        public async Task<IEnumerable<ConversationDto>> GetDoctorConversationsAsync(int doctorId)
        {
            var conversations = await _unitOfWork.GetRepository<Conversation>()
                .FindAsync(c => c.DoctorId == doctorId);

            return conversations.Select(c => MapToDto(c));
        }

        public async Task<MessageDto> SendMessageAsync(
            int conversationId, string senderUserId, SendMessageDto dto)
        {
            var conversation = await _unitOfWork.GetRepository<Conversation>()
                .GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("المحادثة مش موجودة");

            await EnsureUserInConversationAsync(conversation, senderUserId);

            var message = new Message
            {
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                Content = dto.Content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<Message>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return MapMessageToDto(message);
        }

        public async Task<MessageDto> SendAttachmentAsync(
            int conversationId, string senderUserId,
            string attachmentUrl, string attachmentName, string attachmentType, string? caption)
        {
            var conversation = await _unitOfWork.GetRepository<Conversation>()
                .GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("المحادثة مش موجودة");

            await EnsureUserInConversationAsync(conversation, senderUserId);

            var message = new Message
            {
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                Content = caption,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                AttachmentUrl = attachmentUrl,
                AttachmentName = attachmentName,
                AttachmentType = attachmentType
            };

            await _unitOfWork.GetRepository<Message>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return MapMessageToDto(message);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string userId)
        {
            var conversation = await _unitOfWork.GetRepository<Conversation>()
                .GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("المحادثة مش موجودة");

            await EnsureUserInConversationAsync(conversation, userId);

            var messages = await _unitOfWork.GetRepository<Message>()
                .FindAsync(m => m.ConversationId == conversationId);

            return messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => MapMessageToDto(m));
        }

        public async Task MarkMessagesAsReadAsync(int conversationId, string userId)
        {
            var conversation = await _unitOfWork.GetRepository<Conversation>()
                .GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("المحادثة مش موجودة");

            await EnsureUserInConversationAsync(conversation, userId);

            var messages = await _unitOfWork.GetRepository<Message>()
                .FindAsync(m => m.ConversationId == conversationId
                             && m.SenderUserId != userId
                             && !m.IsRead);

            foreach (var message in messages)
            {
                message.IsRead = true;
                _unitOfWork.GetRepository<Message>().Update(message);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // ============ Helpers ============

        private async Task EnsureUserInConversationAsync(Conversation conversation, string userId)
        {
            var patient = await _unitOfWork.GetRepository<Patient>()
                .GetByIdAsync(conversation.PatientId);
            var doctor = await _unitOfWork.GetRepository<Doctor>()
                .GetByIdAsync(conversation.DoctorId);

            bool isPatientSide = patient != null && patient.UserId == userId;
            bool isDoctorSide = doctor != null && doctor.UserId == userId;

            if (!isPatientSide && !isDoctorSide)
                throw new UnauthorizedAccessException("مالكش صلاحية الوصول للمحادثة دي");
        }

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
            CreatedAt = m.CreatedAt,
            AttachmentUrl = m.AttachmentUrl,
            AttachmentName = m.AttachmentName,
            AttachmentType = m.AttachmentType
        };
    }
}
