using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UserByEmailWithRoleSpecification : Specification<User>
    {
        private readonly string email;

        public UserByEmailWithRoleSpecification(string email)
        {
            this.email = email;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return f => f.Email == email;
        }

        public override IQueryable<User> IncludeEntities(IQueryable<User> queryable)
        {
            return queryable.Include(f => f.Role);
        }
    }
}