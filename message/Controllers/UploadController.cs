using Microsoft.AspNetCore.Mvc;
using message.Data;
using message.Entities;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace message.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public UploadController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit; adjust
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // validate content type & extension (very important)
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".txt" }; // extend as needed
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(allowed, e => e == ext))
                return BadRequest("File type not allowed.");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var attachment = new FileAttachment
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Url = $"/uploads/{fileName}",
                Size = file.Length
            };

            _db.FileAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            // return ID so client can pass it to SendMessage()
            return Ok(new { attachment.Id, attachment.Url, attachment.FileName });
        }
    }
}