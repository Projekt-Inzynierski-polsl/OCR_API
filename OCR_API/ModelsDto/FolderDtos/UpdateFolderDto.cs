namespace OCR_API.ModelsDto
{
    public class UpdateFolderDto
    {
        public string? Name { get; set; }
        public string? IconPath { get; set; }
        public string? PasswordToFolder { get; set; } = null;
    }
}
