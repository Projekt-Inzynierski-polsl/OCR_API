namespace OCR_API.Entities
{
    public class UploadedModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int UserId { get; set; }
        public DateTime UploadTime { get; set; }

        public virtual User User { get; set; }
    }
}
