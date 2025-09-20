using Microsoft.AspNetCore.Mvc;
using message.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace message.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MessagesController(AppDbContext db) { _db = db; }

        // GET api/messages/history?otherUserId=...&page=1&pageSize=20
        [HttpGet("history")]
        public async Task<IActionResult> History(string currentUserId, string otherUserId, int page = 1, int pageSize = 30)
        {
            // If otherUserId is null => public chat history
            var query = _db.Messages
                .AsNoTracking()
                .Where(m => m.ReceiverId == null); // public

            if (!string.IsNullOrEmpty(otherUserId))
            {
                // private convo: messages where (sender==me and receiver==other) OR (sender==other and receiver==me)
                query = _db.Messages.Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == currentUserId));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Text,
                    m.SentAt,
                    m.AttachmentId
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items = items.OrderBy(i => i.SentAt) }); // return ascending
        }
    }
}