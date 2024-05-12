

using OCR_API.Entities.Inherits;

namespace OCR_API.Entities
{
    public class BlackListToken : Entity, IHasUserId
    {
        public string Token { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
