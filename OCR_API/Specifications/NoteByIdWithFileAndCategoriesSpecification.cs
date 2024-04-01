using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NoteByIdWithFileAndCategoriesSpecification : Specification<Note>
    {
        private readonly int noteId;

        public NoteByIdWithFileAndCategoriesSpecification(int noteId)
        {
            this.noteId = noteId;
        }

        public override Expression<Func<Note, bool>> ToExpression()
        {
            return f => f.Id == noteId;
        }

        public override IQueryable<Note> IncludeEntities(IQueryable<Note> queryable)
        {
            return queryable.Include(f => f.Categories).Include(f => f.NoteFile);
        }
    }
}
