using OCR_API.Entities;
using OCR_API.ModelsDto.SharedDtos;

namespace OCR_API.ModelsDto
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public bool HasPassword { get; set; }
        public virtual ICollection<NoteDto> Notes { get; set; } = new List<NoteDto>();
        public virtual ICollection<SharedDto> SharedObjects { get; set; } = new List<SharedDto>();
    }
}
