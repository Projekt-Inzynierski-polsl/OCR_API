using Microsoft.AspNetCore.Identity;

namespace OCR_API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nick { get; set; }
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        public virtual Role Role { get; set; }

    }
}
