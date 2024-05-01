using OCR_API.Authorization;

namespace OCR_API.Entities
{
    public class Folder : IHasUserId, IResourceOperationAccess
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string? PasswordHash { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<Shared> SharedObjects { get; set; } = new List<Shared>();
    }
}
