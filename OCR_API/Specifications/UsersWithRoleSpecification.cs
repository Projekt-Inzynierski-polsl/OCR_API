using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UsersWithRoleSpecification : Specification<User>
    {
        private readonly string? searchPhrase;

        public UsersWithRoleSpecification(string? searchPhrase)
        {
            this.searchPhrase = searchPhrase;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return f => (searchPhrase == null || f.Nickname.Contains(searchPhrase, StringComparison.CurrentCultureIgnoreCase));
        }

        public override IQueryable<User> IncludeEntities(IQueryable<User> queryable)
        {
            return queryable.Include(f => f.Role);
        }
    }
}
