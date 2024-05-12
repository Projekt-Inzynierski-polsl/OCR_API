using Microsoft.AspNetCore.Identity;
using OCR_API.Entities.Inherits;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace OCR_API.Entities
{
    public class User : Entity
    {
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
        public virtual ICollection<BlackListToken> BlackListedTokens { get; set; } = new List<BlackListToken>();
        public virtual ICollection<NoteWordError> Errors { get; set; } = new List<NoteWordError>();
        public virtual ICollection<Shared> SharedObjects { get; set; } = new List<Shared>();
        public virtual ICollection<NoteFile> NoteFiles { get; set; } = new List<NoteFile>();
    }
}
