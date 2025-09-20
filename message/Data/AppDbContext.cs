using Microsoft.EntityFrameworkCore;
using message.Entities;

namespace message.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfile> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Indexes, relationships, etc.
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Attachment)
                .WithOne(a => a.Message)
                .HasForeignKey<FileAttachment>(a => a.MessageId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}