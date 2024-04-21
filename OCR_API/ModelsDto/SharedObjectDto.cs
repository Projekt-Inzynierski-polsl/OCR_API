using OCR_API.Enums;

namespace OCR_API.ModelsDto
{
    public class SharedObjectDto
    {
        public string Email { get; set; }
        public int ObjectId { get; set; }
        public EShareMode ShareMode { get; set; } = EShareMode.View;
        public string? Password { get; set; }

    }
}
