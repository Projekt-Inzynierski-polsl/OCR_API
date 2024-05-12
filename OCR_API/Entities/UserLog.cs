

using OCR_API.Entities.Inherits;

namespace OCR_API.Entities
{
    public class UserLog : Entity, IHasUserId
    {
        public int UserId { get; set; }
        public int ActionId { get; set; }
        public int? ObjectId { get; set; } = null;
        public DateTime LogTime { get; set; }

        public virtual User User { get; set; }
        public virtual UserAction Action { get; set; }

    }
}
