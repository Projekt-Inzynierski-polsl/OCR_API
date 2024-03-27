namespace OCR_API.Entities
{
    public class NoteWorldError
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public string CorrectContent { get; set; }

        public virtual NoteLine Line { get; set; }
    }
}
