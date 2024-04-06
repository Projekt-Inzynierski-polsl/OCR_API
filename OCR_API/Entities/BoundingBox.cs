using System.Drawing;

namespace OCR_API.Entities
{
    public class BoundingBox
    {
        public int Id { get; set; }
        public int NoteFileId { get; set; }
        public string Coordinates { get; set; }
        public virtual NoteFile NoteFile { get; set; }
        public virtual ICollection<NoteLine> Lines { get; set; } = new List<NoteLine>();
    }
}
