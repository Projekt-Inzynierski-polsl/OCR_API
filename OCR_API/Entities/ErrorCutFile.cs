using OCR_API.Entities.Inherits;

namespace OCR_API.Entities
{
    public class ErrorCutFile : Entity, IHasHashedKey
    {
        public string Path { get; set; }
        public string? HashedKey { get; set; } = null;
        public virtual ICollection<NoteWordError> Errors { get; set; } = new List<NoteWordError>();
    }
}