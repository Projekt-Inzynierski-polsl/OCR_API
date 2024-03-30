using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UserFoldersWithNotesSpecification : Specification<Folder>
    {
        private readonly int userId;

        public UserFoldersWithNotesSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<Folder, bool>> ToExpression()
        {
            return f => f.UserId == userId;
        }

        public override IQueryable<Folder> IncludeEntities(IQueryable<Folder> queryable)
        {
            return queryable.Include(f => f.PasswordHash == null ? f.Notes : null);
        }
    }
}
