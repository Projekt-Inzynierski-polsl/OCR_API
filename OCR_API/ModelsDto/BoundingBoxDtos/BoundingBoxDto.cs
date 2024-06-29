using OCR_API.ModelsDto.NoteLineDtos;

namespace OCR_API.ModelsDto.BoundingBoxDtos
{
    public class BoundingBoxDto
    {
        public int LeftX { get; set; }
        public int LeftY { get; set; }
        public int RightX { get; set; }
        public int RightY { get; set; }
        public List<NoteLineDto> Lines { get; set; }
    }
}