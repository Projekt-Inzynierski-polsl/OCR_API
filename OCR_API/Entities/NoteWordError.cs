using OCR_API.Entities.Inherits;

namespace OCR_API.Entities
{
    public class NoteWordError : Entity, IHasUserId
    {
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string CorrectContent { get; set; }
        public virtual ErrorCutFile File { get; set; }
        public virtual User User { get; set; }
    }
}
