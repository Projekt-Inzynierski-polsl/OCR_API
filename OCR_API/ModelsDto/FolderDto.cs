using OCR_API.Entities;

namespace OCR_API.ModelsDto
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public bool hasPassword { get; set; }
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
