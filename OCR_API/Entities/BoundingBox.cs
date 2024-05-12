namespace OCR_API.Entities
{
    public class BoundingBox : Entity
    {
        public int NoteFileId { get; set; }
        public string Coordinates { get; set; }
        public virtual NoteFile NoteFile { get; set; }
        public virtual ICollection<NoteLine> Lines { get; set; } = new List<NoteLine>();
    }
}
