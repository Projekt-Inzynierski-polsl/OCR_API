using Microsoft.AspNetCore.Identity;

namespace OCR_API.Entities
{
    public class User
    {
        public uint Id { get; set; }
        public string Email { get; set; }
        public string Nick { get; set; }
        public string PasswordHash { get; set; }

        public uint RoleId { get; set; }
        public int Test {  get; set; }

        public virtual Role Role { get; set; }

    }
}
