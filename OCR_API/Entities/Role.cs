

namespace OCR_API.Entities
{
    public class Role : Entity
    {
        public string Name { get; set; }
        public IEnumerable<User> Users { get; set; }

    }
}
