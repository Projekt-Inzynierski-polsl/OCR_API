using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class SharesByFolderIdSpecification : Specification<Shared>
    {
        private int folderId { get; }

        public SharesByFolderIdSpecification(int folderId)
        {
            this.folderId = folderId;
        }

        public override Expression<Func<Shared, bool>> ToExpression()
        {
            return f => f.FolderId == folderId;
        }

        public override IQueryable<Shared> IncludeEntities(IQueryable<Shared> queryable)
        {
            return queryable.Include(f => f.Mode).Include(f => f.User);
        }
    }
}