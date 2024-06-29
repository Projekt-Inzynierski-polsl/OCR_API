using OCR_API.ModelsDto.BoundingBoxDtos;

namespace OCR_API.ModelsDto.NoteFileDtos
{
    public class NoteFileDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public virtual ICollection<BoundingBoxDto> BoundingBoxes { get; set; } = new List<BoundingBoxDto>();
    }
}