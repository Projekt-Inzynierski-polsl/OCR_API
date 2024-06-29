using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class SharesByNoteIdSpecification : Specification<Shared>
    {
        private int noteId { get; }

        public SharesByNoteIdSpecification(int noteId)
        {
            this.noteId = noteId;
        }

        public override Expression<Func<Shared, bool>> ToExpression()
        {
            return f => f.NoteId == noteId;
        }

        public override IQueryable<Shared> IncludeEntities(IQueryable<Shared> queryable)
        {
            return queryable.Include(f => f.Mode).Include(f => f.User);
        }
    }
}