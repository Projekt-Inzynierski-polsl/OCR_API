namespace OCR_API.Entities
{
    public class NoteFile
    {
        public int Id { get; set; }
        public string Path { get; set; } 
        public virtual ICollection<BoundingBox> BoundingBoxes { get; set; } = new List<BoundingBox>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
