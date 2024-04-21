namespace OCR_API.Entities
{
    public class ShareMode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Shared> SharedObjects { get; set; }
    }
}
