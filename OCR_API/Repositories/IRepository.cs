using Microsoft.EntityFrameworkCore;

namespace OCR_API.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        DbSet<TEntity> Entity { get; }
        void Add(TEntity entity);
        void Remove(int id);
        TEntity GetById(int id);
        List<TEntity> GetAll();
    }
}
