namespace OCR_API.ModelsDto
{
    public class AddErrorDto
    {
        public int FileId { get; set; }
        public string CorrectContent { get; set; }
        public string WrongContent { get; set; }
        public int LeftX { get; set; }
        public int LeftY { get; set; }
        public int RightX { get; set; }
        public int RightY { get; set; }
    }
}
