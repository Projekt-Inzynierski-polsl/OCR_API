using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NoteCategoriesByUserId : Specification<NoteCategory>
    {
        private readonly int userId;

        public NoteCategoriesByUserId(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<NoteCategory, bool>> ToExpression()
        {
            return f => f.UserId == userId;
        }
    }
}
