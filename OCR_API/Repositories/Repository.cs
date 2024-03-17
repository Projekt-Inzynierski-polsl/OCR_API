﻿using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;

namespace OCR_API.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly SystemDbContext dbContext;

        public Repository(SystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DbSet<TEntity> Entity => dbContext.Set<TEntity>();

        public void Add(TEntity entity)
        {
            Entity.Add(entity);
        }

        public void Remove(int id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                dbContext.Remove(entity);
            }

        }

        public TEntity GetById(int id)
        {
            return Entity.Find(id);
        }

        public List<TEntity> GetAll()
        {
            return Entity.ToList();
        }
    }
}
