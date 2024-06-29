namespace OCR_API.ModelsDto
{
    public class AddNoteDto
    {
        public int? FolderId { get; set; } = null;
        public int NoteFileId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; } = string.Empty;

        public int[] CategoriesIds { get; set; }
    }
}