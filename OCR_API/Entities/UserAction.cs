

namespace OCR_API.Entities
{
    public class UserAction : Entity
    {
        public string Name { get; set; }

        public virtual ICollection<UserLog> Logs { get; set; } = new List<UserLog>();
    }
}
