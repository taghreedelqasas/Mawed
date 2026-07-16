using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Maw3ed.APIs.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId,
                conversationId.ToString());
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId,
                conversationId.ToString());
        }

        public async Task SendMessage(int conversationId, object message)
        {
            await Clients.Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", message);
        }

        public async Task UserStartedTyping(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            await Clients.OthersInGroup(conversationId.ToString())
                .SendAsync("UserTyping", conversationId, userId);
        }

        public async Task UserStoppedTyping(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            await Clients.OthersInGroup(conversationId.ToString())
                .SendAsync("UserStoppedTyping", conversationId, userId);
        }

        public async Task MarkAsRead(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            await Clients.OthersInGroup(conversationId.ToString())
                .SendAsync("MessagesRead", conversationId, userId);
        }
    }
}