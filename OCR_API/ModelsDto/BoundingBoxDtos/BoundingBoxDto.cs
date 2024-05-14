using OCR_API.ModelsDto.NoteLineDtos;

namespace OCR_API.ModelsDto.BoundingBoxDtos
{
    public class BoundingBoxDto
    {
        public Coords Coordinates { get; set; }
        public List<NoteLineDto> Lines { get; set; }
    }
}
