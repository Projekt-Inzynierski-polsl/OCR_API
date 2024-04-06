﻿namespace OCR_API.Entities
{
    public class BlackListToken : IHasUserId
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
