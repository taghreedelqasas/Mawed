using Maw3ed.BLL.DTOs.ConversationDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Maw3ed.APIs.Hubs
{
    [Authorize] // كل الـ Hub محتاج Login
    public class ChatHub : Hub
    {
        private readonly IConversationService _conversationService;

        public ChatHub(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        // لما اليوزر يفتح المحادثة
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId,
                conversationId.ToString());
        }

        // لما اليوزر يقفل المحادثة
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId,
                conversationId.ToString());
        }

        // لما اليوزر يبعت رسالة
        public async Task SendMessage(int conversationId, string content)
        {
            // بياخد الـ userId من الـ Token
            var senderUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderUserId == null)
            {
                await Clients.Caller.SendAsync("Error", "مش مسموح، لازم تعمل Login الأول");
                return;
            }

            var dto = new SendMessageDto { Content = content };

            // احفظ الرسالة في الـ DB
            var message = await _conversationService
                .SendMessageAsync(conversationId, senderUserId, dto);

            // ابعت الرسالة لكل الناس في الغرفة دي فوراً
            await Clients.Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", message);
        }
    }
}