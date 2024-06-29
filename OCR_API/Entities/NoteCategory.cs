using OCR_API.Entities.Inherits;

namespace OCR_API.Entities
{
    public class NoteCategory : Entity, IHasUserId
    {
        public string Name { get; set; }
        public string? HexColor { get; set; } = null;
        public int UserId { get; set; }
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual User User { get; set; }
    }
}