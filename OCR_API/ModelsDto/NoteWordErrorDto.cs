using OCR_API.Entities;

namespace OCR_API.ModelsDto
{
    public class NoteWordErrorDto
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string CorrectContent { get; set; }
        public string WrongContent { get; set; }
        public bool IsAccepted { get; set; }
    }
}
