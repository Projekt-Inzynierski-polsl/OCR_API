

namespace OCR_API.Entities
{
    public class UploadedModel : Entity
    {
        public string Path { get; set; }
        public int UserId { get; set; }
        public DateTime UploadTime { get; set; }

        public virtual User User { get; set; }
    }
}
