using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UserFoldersSpecification : Specification<Folder>
    {
        private readonly int userId;

        public UserFoldersSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<Folder, bool>> ToExpression()
        {
            return f => f.UserId == userId;
        }
    }
}
