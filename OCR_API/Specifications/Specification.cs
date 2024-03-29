using System.Linq.Expressions;

namespace OCR_API.Specifications
{
    public abstract class Specification<T>
    {
        public abstract Expression<Func<T, bool>> ToExpression();

        public virtual IQueryable<T> IncludeEntities(IQueryable<T> queryable)
        {
            return queryable;
        }
    }
}
