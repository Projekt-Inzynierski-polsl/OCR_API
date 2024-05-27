namespace OCR_API.ModelsDto
{
    public class UpdateUserDto
    {
        public string? Email { get; set; } = null;
        public string? Nickname { get; set; } = null;
        public string? Password { get; set; } = null;
        public int? RoleId { get; set; } = null;

    }
}
