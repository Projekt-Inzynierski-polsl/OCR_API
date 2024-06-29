using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class SharedNotesWithFileAndCategoriesSpecification : Specification<Shared>
    {
        private readonly int userId;

        public SharedNotesWithFileAndCategoriesSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<Shared, bool>> ToExpression()
        {
            return f => f.UserId == userId && f.NoteId != null;
        }

        public override IQueryable<Shared> IncludeEntities(IQueryable<Shared> queryable)
        {
            return queryable.Include(f => f.Note).ThenInclude(f => f.Categories);
        }
    }
}