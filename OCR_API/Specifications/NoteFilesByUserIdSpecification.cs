using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class NoteFilesByUserIdSpecification : Specification<NoteFile>
    {
        private readonly int userId;
        private readonly string? searchPhrase;

        public NoteFilesByUserIdSpecification(int userId, string? searchPhrase)
        {
            this.userId = userId;
            this.searchPhrase = searchPhrase;
        }

        public override Expression<Func<NoteFile, bool>> ToExpression()
        {
            return f => f.UserId == userId && (searchPhrase == null || f.Path.ToLower().Contains(searchPhrase.ToLower()));
        }
    }
}