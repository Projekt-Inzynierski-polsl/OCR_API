using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NoteByIdWithFileAndCategoriesAndSharedSpecification : Specification<Note>
    {
        private readonly int noteId;
        private readonly int userId;

        public NoteByIdWithFileAndCategoriesAndSharedSpecification(int noteId, int userId)
        {
            this.noteId = noteId;
            this.userId = userId;
        }

        public override Expression<Func<Note, bool>> ToExpression()
        {
            return f => f.Id == noteId;
        }

        public override IQueryable<Note> IncludeEntities(IQueryable<Note> queryable)
        {
            return queryable.Include(f => f.Categories)
                .Include(f => f.NoteFile).Include(f => f.SharedObjects.Where(s => s.UserId == userId));
        }
    }
}
