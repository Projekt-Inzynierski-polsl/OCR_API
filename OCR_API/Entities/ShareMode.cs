namespace OCR_API.Entities
{
    public class ShareMode : Entity
    {
        public string Name { get; set; }
        public IEnumerable<Shared> SharedObjects { get; set; }
    }
}