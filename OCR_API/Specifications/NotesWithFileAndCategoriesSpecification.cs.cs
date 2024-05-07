using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NotesWithFileAndCategoriesSpecification :Specification<Note>
    {
        private readonly int userId;
        private readonly string searchPhrase;

        public NotesWithFileAndCategoriesSpecification(int userId, string? searchPhrase)
        {
            this.userId = userId;
            this.searchPhrase = searchPhrase;
        }

        public override Expression<Func<Note, bool>> ToExpression()
        {
            return f => f.UserId == userId && (searchPhrase == null || f.Name.ToLower().Contains(searchPhrase.ToLower()));
        }

        public override IQueryable<Note> IncludeEntities(IQueryable<Note> queryable)
        {
            return queryable.Include(f => f.NoteFile).Include(f => f.Categories);
        }
    }

}
