using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace message.Entities
{
    public class Message
    {
        [Key]
        public long Id { get; set; }

        public string SenderId { get; set; }
        public string ReceiverId { get; set; } // null if public/broadcast; otherwise user id for private
        public string Text { get; set; }
        public DateTime SentAt { get; set; }

        public bool IsFile => AttachmentId != null;
        public long? AttachmentId { get; set; }
        public FileAttachment Attachment { get; set; }
    }
}