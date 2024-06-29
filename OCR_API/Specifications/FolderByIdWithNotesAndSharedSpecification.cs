using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class FolderByIdWithNotesAndSharedSpecification : Specification<Folder>
    {
        private readonly int folderId;
        private readonly int userId;

        public FolderByIdWithNotesAndSharedSpecification(int folderId, int userId)
        {
            this.folderId = folderId;
            this.userId = userId;
        }

        public override Expression<Func<Folder, bool>> ToExpression()
        {
            return f => f.Id == folderId;
        }

        public override IQueryable<Folder> IncludeEntities(IQueryable<Folder> queryable)
        {
            return queryable.Include(f => f.Notes).Include(f => f.SharedObjects.Where(s => s.UserId == userId));
        }
    }
}