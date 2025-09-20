using Microsoft.AspNetCore.SignalR;
using message.Data;
using message.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace message.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        // 🔹 Register connection to a "user-{id}" group
        public async Task Register(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        // 🔹 Send a message (public or private) with persistence
        public async Task SendMessage(string senderId, string receiverId, string text, long? attachmentId = null)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = string.IsNullOrWhiteSpace(receiverId) ? null : receiverId,
                Text = text,
                SentAt = DateTime.UtcNow,
                AttachmentId = attachmentId
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            // Minimal DTO for clients
            var dto = new
            {
                message.Id,
                message.SenderId,
                message.ReceiverId,
                message.Text,
                SentAt = message.SentAt,
                message.AttachmentId
            };

            if (message.ReceiverId == null)
            {
                // Public chat → broadcast to all
                await Clients.All.SendAsync("ReceiveMessage", dto);
            }
            else
            {
                // Private chat → only sender & receiver groups
                await Clients.Group($"user-{message.ReceiverId}").SendAsync("ReceiveMessage", dto);
                await Clients.Group($"user-{message.SenderId}").SendAsync("ReceiveMessage", dto);
            }
        }

        // 🔹 Typing indicators
        public async Task Typing(string userId, string receiverId, bool isTyping)
        {
            if (string.IsNullOrEmpty(receiverId))
            {
                await Clients.Others.SendAsync("UserTyping", userId, isTyping);
            }
            else
            {
                await Clients.Group($"user-{receiverId}").SendAsync("UserTyping", userId, isTyping);
            }
        }

        // 🔹 Connection handling
        public override async Task OnConnectedAsync()
        {
            // You can auto-register here if you map Context.UserIdentifier
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Optionally: cleanup groups on disconnect
            await base.OnDisconnectedAsync(exception);
        }
    }
}