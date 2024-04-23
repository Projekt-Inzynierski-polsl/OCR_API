namespace OCR_API.Entities
{
    public class Shared
    {
        public int Id { get; set; }
        public int? UserId { get; set; } = null;
        public int? FolderId { get; set; } = null;
        public int? NoteId { get; set; } = null;
        public int ModeId { get; set; } = 1;

        public virtual User User { get; set; }
        public virtual Folder? Folder { get; set; }
        public virtual Note? Note { get; set; }
        public virtual ShareMode Mode { get; set; }
    }
}
