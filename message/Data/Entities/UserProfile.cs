using System.ComponentModel.DataAnnotations;

namespace message.Entities
{
    public class UserProfile
    {
        [Key]
        public string Id { get; set; } // string to match Identity user id if used
        public string UserName { get; set; }
        // Add more fields as needed (DisplayName, AvatarUrl, etc.)
    }
}