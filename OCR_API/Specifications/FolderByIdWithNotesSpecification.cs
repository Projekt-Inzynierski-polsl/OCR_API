using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class FolderByIdWithNotesSpecification : Specification<Folder>
    {
        private readonly int folderId;

        public FolderByIdWithNotesSpecification(int folderId)
        {
            this.folderId = folderId;
        }

        public override Expression<Func<Folder, bool>> ToExpression()
        {
            return f => f.Id == folderId;
        }

        public override IQueryable<Folder> IncludeEntities(IQueryable<Folder> queryable)
        {
            return queryable.Include(f => f.Notes);
        }
    }
}
