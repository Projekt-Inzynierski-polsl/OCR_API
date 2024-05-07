namespace OCR_API.Entities
{
    public class NoteLine
    {
        public int Id { get; set; }
        public int BoundingBoxId { get; set; }
        public string Coordinates { get; set; }
        public virtual BoundingBox BoundingBox { get; set; }
    }
}
