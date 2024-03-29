namespace OCR_API.ModelsDto
{
    public class AddFolderDto
    {
        public string Name { get; set; }
        public string? IconPath { get; set; }
        public string? Password { get; set; }
        public string? ConfirmedPassword { get; set; }
    }
}
