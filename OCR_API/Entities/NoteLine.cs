using DocumentFormat.OpenXml.Office.CoverPageProps;


namespace OCR_API.Entities
{
    public class NoteLine : Entity
    {
        public int BoundingBoxId { get; set; }
        public string Coordinates { get; set; }
        public string Content { get; set; }
        public virtual BoundingBox BoundingBox { get; set; }
    }
}
