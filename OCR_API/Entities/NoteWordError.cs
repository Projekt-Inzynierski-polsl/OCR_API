namespace OCR_API.Entities
{
    public class NoteWordError : IHasUserId
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string CorrectContent { get; set; }
        public virtual ErrorCutFile File { get; set; }
        public virtual User User { get; set; }
    }
}
