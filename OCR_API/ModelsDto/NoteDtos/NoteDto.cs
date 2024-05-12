using OCR_API.Entities;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.ModelsDto.NoteFileDtos;

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
        public virtual ICollection<NoteCategoryDto> Categories { get; set; } = new List<NoteCategoryDto>();
    }
}
