namespace OCR_API.Entities
{
    public class NoteLine : Entity
    {
        public int BoundingBoxId { get; set; }
        public int LeftX { get; set; }
        public int LeftY { get; set; }
        public int RightX { get; set; }
        public int RightY { get; set; }
        public string Content { get; set; }
        public virtual BoundingBox BoundingBox { get; set; }
    }
}