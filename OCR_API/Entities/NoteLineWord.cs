namespace OCR_API.Entities
{
    public class NoteLineWord
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public string Content { get; set; }
        public string Coordinates { get; set; }

        public virtual NoteLine NoteLine { get; set; }
    }
}
