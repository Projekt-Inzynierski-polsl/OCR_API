using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NoteCategoriesByUserIdSpecification : Specification<NoteCategory>
    {
        private readonly int userId;
        private readonly string? searchPhrase;

        public NoteCategoriesByUserIdSpecification(int userId, string? searchPhrase)
        {
            this.userId = userId;
            this.searchPhrase = searchPhrase;
        }

        public override Expression<Func<NoteCategory, bool>> ToExpression()
        {
            return f => f.UserId == userId && (searchPhrase == null || f.Name.ToLower().Contains(searchPhrase.ToLower()));
        }
    }
}