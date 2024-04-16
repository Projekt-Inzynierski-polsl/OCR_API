using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.Logger;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UserLogByTypeAndDateSpecification : Specification<UserLog>
    {
        private readonly EUserAction type;
        private readonly DateTime start;
        private readonly DateTime end;
        private readonly int userId;

        public UserLogByTypeAndDateSpecification(EUserAction type, DateTime start, DateTime end, int userId)
        {
            this.type = type;
            this.start = start;
            this.end = end;
            this.userId = userId;
        }

        public override Expression<Func<UserLog, bool>> ToExpression()
        {
            if(userId != 0)
            {
                return f => f.ActionId == (int)type && f.LogTime > start && f.LogTime < end;
            }
            else
            {
                return f => f.ActionId == (int)type && f.LogTime > start && f.LogTime < end && f.UserId == userId;
            }
            
        }
    }
}
