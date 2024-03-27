using Microsoft.AspNetCore.Identity;

namespace OCR_API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Folder> Folders { get; set; } = new List<Folder>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<UploadedModel> UploadedModels { get; set; } = new List<UploadedModel>();
        public virtual ICollection<UserLog> Logs { get; set; } = new List<UserLog>();
        public virtual ICollection<NoteCategory> NoteCategories { get; set; } = new List<NoteCategory>();
    }
}
