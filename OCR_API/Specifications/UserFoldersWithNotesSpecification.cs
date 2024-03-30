using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UserFoldersWithNotesSpecification : Specification<Folder>
    {
        private readonly int userId;

        public UserFoldersWithNotesSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<Folder, bool>> ToExpression()
        {
            return f => f.UserId == userId;
        }

        public override IQueryable<Folder> IncludeEntities(IQueryable<Folder> queryable)
        {
            var foldersWithNotes = queryable
                .Where(f => f.PasswordHash == null)
                .Include(f => f.Notes)
                .ToList(); 

            var foldersWithoutNotes = queryable
                .Where(f => f.PasswordHash != null)
                .ToList();

            var allFolders = foldersWithNotes.Concat(foldersWithoutNotes);

            var sortedFolders = allFolders.OrderBy(f => f.Id);

            return sortedFolders.AsQueryable();
        }
    }
}
