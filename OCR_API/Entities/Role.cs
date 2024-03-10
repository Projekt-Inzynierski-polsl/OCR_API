namespace OCR_API.Entities
{
    public class Role
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<User> Users { get; set; }

    }
}
