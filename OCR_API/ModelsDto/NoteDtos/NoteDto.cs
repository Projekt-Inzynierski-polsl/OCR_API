using OCR_API.Entities;

namespace OCR_API.ModelsDto
{
    public class NoteDto
    {
        public int Id { get; set; }
        public int FolderId { get; set; }
        public int NoteFileId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsPrivate { get; set; }
        public virtual NoteFile NoteFile { get; set; }
        public virtual ICollection<NoteCategory> Categories { get; set; } = new List<NoteCategory>();
    }
}
