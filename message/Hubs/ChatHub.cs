using Microsoft.AspNetCore.SignalR;
using message.Data;
using message.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace message.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        // 🔹 Register a connection to "user-{id}" group
        public async Task Register(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        // 🔹 Send a message (with optional persistence + private/public)
        public async Task SendMessage(string senderId, string receiverId, string text, long? attachmentId = null)
        {
            if (string.IsNullOrWhiteSpace(text) && attachmentId == null)
                return; // Ignore empty messages with no attachments

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

            // Minimal DTO for clients (avoid sending navigation props)
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
                // Public chat → broadcast to everyone
                await Clients.All.SendAsync("ReceiveMessage", dto);
            }
            else
            {
                // Private chat → deliver to sender & receiver groups
                await Clients.Group($"user-{message.ReceiverId}").SendAsync("ReceiveMessage", dto);
                await Clients.Group($"user-{message.SenderId}").SendAsync("ReceiveMessage", dto);
            }
        }

        // 🔹 Typing indicator (notify public or private receiver)
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

        // 🔹 On connection (optional auto-register if you use Claims/UserIdentifier)
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // 🔹 On disconnection
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}