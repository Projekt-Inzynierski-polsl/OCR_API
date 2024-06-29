namespace OCR_API.ModelsDto.SharedDtos
{
    public class SharedDto
    {
        public int? UserId { get; set; } = null;
        public int? FolderId { get; set; } = null;
        public int? NoteId { get; set; } = null;
        public int ModeId { get; set; } = 1;
        public virtual NoteDto Note { get; set; }
        public virtual FolderDto Folder { get; set; }
    }
}