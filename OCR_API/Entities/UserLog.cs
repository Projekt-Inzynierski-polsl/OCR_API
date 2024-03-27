namespace OCR_API.Entities
{
    public class UserLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ActionId { get; set; }
        public string Description { get; set; }
        public DateTime LogTime { get; set; }

        public virtual User User { get; set; }
        public virtual UserAction Action { get; set; }

    }
}
