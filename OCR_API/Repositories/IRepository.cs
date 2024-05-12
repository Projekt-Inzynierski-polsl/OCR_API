using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.Specifications;

namespace OCR_API.Repositories
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        DbSet<TEntity> Entity { get; }
        void Add(TEntity entity);
        void Remove(int id);
        TEntity GetById(int id);
        TEntity GetByIdAndUserId(int id, int userId);
        List<TEntity> GetAll();
        List<TEntity> GetAllByUser(int userId);
        IQueryable<TEntity> GetBySpecification(Specification<TEntity> spec);
    }
}
