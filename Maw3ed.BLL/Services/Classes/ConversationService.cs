using Maw3ed.BLL.DTOs.ConversationDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;

        public ConversationService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ConversationDto> GetOrCreateConversationAsync(int patientId, int doctorId, string currentUserId)
        {
            var existing = await _context.Set<Conversation>()
                .Where(c => c.PatientId == patientId && c.DoctorId == doctorId)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync();

            if (existing != null)
                return MapToDto(existing, currentUserId);

            var newConversation = new Conversation
            {
                PatientId = patientId,
                DoctorId = doctorId
            };

            await _unitOfWork.GetRepository<Conversation>().AddAsync(newConversation);
            await _unitOfWork.SaveChangesAsync();

            var saved = await _context.Set<Conversation>()
                .Where(c => c.Id == newConversation.Id)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .Include(c => c.Messages)
                .FirstAsync();

            return MapToDto(saved, currentUserId);
        }

        public async Task<IEnumerable<ConversationDto>> GetPatientConversationsAsync(int patientId, string currentUserId)
        {
            var conversations = await _context.Set<Conversation>()
                .Where(c => c.PatientId == patientId)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.CreatedAt))
                .ToListAsync();

            return conversations.Select(c => MapToDto(c, currentUserId));
        }

        public async Task<IEnumerable<ConversationDto>> GetDoctorConversationsAsync(int doctorId, string currentUserId)
        {
            var conversations = await _context.Set<Conversation>()
                .Where(c => c.DoctorId == doctorId)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.CreatedAt))
                .ToListAsync();

            return conversations.Select(c => MapToDto(c, currentUserId));
        }

        public async Task<MessageDto> SendMessageAsync(
            int conversationId, string senderUserId, SendMessageDto dto)
        {
            var conversation = await _context.Set<Conversation>()
                .Where(c => c.Id == conversationId)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync();

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

            return MapMessageToDto(message, conversation, senderUserId);
        }

        public async Task<MessageDto> SendAttachmentAsync(
            int conversationId, string senderUserId,
            string attachmentUrl, string attachmentName, string attachmentType, string? caption)
        {
            var conversation = await _context.Set<Conversation>()
                .Where(c => c.Id == conversationId)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync();

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

            return MapMessageToDto(message, conversation, senderUserId);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string userId)
        {
            var conversation = await _context.Set<Conversation>()
                .Where(c => c.Id == conversationId)
                .Include(c => c.Doctor).ThenInclude(d => d!.User)
                .Include(c => c.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync();

            if (conversation == null)
                throw new Exception("المحادثة مش موجودة");

            await EnsureUserInConversationAsync(conversation, userId);

            var messages = await _unitOfWork.GetRepository<Message>()
                .FindAsync(m => m.ConversationId == conversationId);

            return messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => MapMessageToDto(m, conversation, userId));
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

        private ConversationDto MapToDto(Conversation c, string currentUserId)
        {
            var lastMessage = c.Messages?
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            return new ConversationDto
            {
                Id = c.Id,
                PatientId = c.PatientId,
                DoctorId = c.DoctorId,
                DoctorName = c.Doctor?.User != null
                    ? $"{c.Doctor.User.FirstName} {c.Doctor.User.LastName}".Trim()
                    : null,
                DoctorImage = c.Doctor?.ImageProfile,
                PatientName = c.Patient?.User != null
                    ? $"{c.Patient.User.FirstName} {c.Patient.User.LastName}".Trim()
                    : null,
                LastMessage = lastMessage?.Content,
                LastMessageAt = lastMessage?.CreatedAt,
                UnreadCount = c.Messages?.Count(m => !m.IsRead) ?? 0,
                Messages = c.Messages?.Select(m => MapMessageToDto(m, c, currentUserId)).ToList()
                           ?? new List<MessageDto>()
            };
        }

        private MessageDto MapMessageToDto(Message m, Conversation c, string currentUserId)
        {
            string role = "Patient";
            if (c.Doctor?.User != null && c.Doctor.UserId == m.SenderUserId)
                role = "Doctor";

            return new MessageDto
            {
                Id = m.Id,
                SenderUserId = m.SenderUserId,
                SenderRole = role,
                IsMine = m.SenderUserId == currentUserId,
                Content = m.Content,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt,
                AttachmentUrl = m.AttachmentUrl,
                AttachmentName = m.AttachmentName,
                AttachmentType = m.AttachmentType
            };
        }
    }
}
