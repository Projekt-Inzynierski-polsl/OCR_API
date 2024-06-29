using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class SharedFoldersWithNotesSpecification : Specification<Shared>
    {
        private readonly int userId;

        public SharedFoldersWithNotesSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<Shared, bool>> ToExpression()
        {
            return f => f.UserId == userId && f.FolderId != null;
        }

        public override IQueryable<Shared> IncludeEntities(IQueryable<Shared> queryable)
        {
            var sharedFolders = queryable
                .Include(f => f.Folder)
                .ToList();

            return sharedFolders.AsQueryable();
        }
    }
}