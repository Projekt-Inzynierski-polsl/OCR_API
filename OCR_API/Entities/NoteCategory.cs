using OCR_API.Authorization;

namespace OCR_API.Entities
{
    public class NoteCategory : IHasUserId
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? HexColor { get; set; } = null;
        public int UserId { get; set; }
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual User User { get; set; }
    }
}
