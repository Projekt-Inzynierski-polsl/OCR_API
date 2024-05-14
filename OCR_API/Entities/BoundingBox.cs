using OCR_API.ModelsDto;

namespace OCR_API.Entities
{
    public class BoundingBox : Entity
    {
        public int NoteFileId { get; set; }
        public int LeftX { get; set; }
        public int LeftY { get; set; }
        public int RightX { get; set; }
        public int RightY { get; set; }
        public virtual NoteFile NoteFile { get; set; }
        public virtual ICollection<NoteLine> Lines { get; set; } = new List<NoteLine>();
    }
}
