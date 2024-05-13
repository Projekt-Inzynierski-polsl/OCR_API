namespace OCR_API.ModelsDto.UserLogDtos
{
    public class UserLogDto
    {
        public int UserId { get; set; }
        public int ActionId { get; set; }
        public int? ObjectId { get; set; } = null;
        public DateTime LogTime { get; set; }
    }
}
