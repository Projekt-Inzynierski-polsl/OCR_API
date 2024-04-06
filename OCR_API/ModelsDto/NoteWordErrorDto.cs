using OCR_API.Entities;

namespace OCR_API.ModelsDto
{
    public class NoteWordErrorDto
    {
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string CorrectContent { get; set; }
    }
}
