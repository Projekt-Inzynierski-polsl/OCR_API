namespace OCR_API.ModelsDto.NoteFileDtos
{
    public class UploadFileDto
    {
        public IFormFile Image { get; set; }
        public string BoundingBoxes { get; set; }
    }
}