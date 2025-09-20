using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace message.Entities
{
    public class FileAttachment
    {
        [Key]
        public long Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; } // where it's stored (wwwroot/uploads/...)
        public long Size { get; set; }
        public long? MessageId { get; set; }
        public Message Message { get; set; }
    }
}