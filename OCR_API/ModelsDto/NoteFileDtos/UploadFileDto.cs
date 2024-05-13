using OCR_API.Entities;
using OCR_API.ModelsDto.BoundingBoxDtos;

namespace OCR_API.ModelsDto.NoteFileDtos
{
    public class UploadFileDto
    {
        public IFormFile Image { get; set; }
        public List<Coords> BoundingBoxes { get; set; }
    }
}
