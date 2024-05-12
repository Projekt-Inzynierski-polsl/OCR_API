namespace OCR_API.ModelsDto
{
    public class AddErrorDto
    {
        public int FileId { get; set; }
        public string CorrectContent { get; set; }
        public Coords Coordinates { get; set; }
    }
}
