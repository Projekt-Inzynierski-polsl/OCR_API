namespace OCR_API.Entities
{
    public class ErrorCutFile
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public virtual ICollection<NoteWordError> Errors { get; set; } = new List<NoteWordError>();
    }
}
