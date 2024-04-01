using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NotesWithFileAndCategoriesSpecification :Specification<Note>
    {
        private readonly int userId;

        public NotesWithFileAndCategoriesSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<Note, bool>> ToExpression()
        {
            return f => f.UserId == userId;
        }

        public override IQueryable<Note> IncludeEntities(IQueryable<Note> queryable)
        {
            return queryable.Include(f => f.NoteFile).Include(f => f.Categories);
        }
    }

}
