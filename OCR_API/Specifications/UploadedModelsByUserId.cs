using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public class UploadedModelsByUserIdSpecification : Specification<UploadedModel>
    {
        private readonly int userId;

        public UploadedModelsByUserIdSpecification(int userId)
        {
            this.userId = userId;
        }

        public override Expression<Func<UploadedModel, bool>> ToExpression()
        {
            return f => f.UserId == userId;
        }
    }
}
