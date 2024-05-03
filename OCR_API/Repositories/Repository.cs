﻿using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Specifications;

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
            else
            {
                throw new NotFoundException("That entity doesn't exist.");
            }

        }

        public TEntity GetById(int id)
        {
            TEntity? entity = Entity.Find(id);
            return entity is null ? throw new NotFoundException("That entity doesn't exist.") : entity;
        }

        public List<TEntity> GetAll()
        {
            return Entity.ToList();
        }

        public List<TEntity> GetAllByUser(int userId)
        {
            if (typeof(TEntity).GetInterfaces().Contains(typeof(IHasUserId)))
            {
                // Jeśli tak, wykonaj zapytanie z filtrowaniem UserId
                return Entity.AsQueryable()
                    .Cast<IHasUserId>()
                    .Where(e => e.UserId == userId)
                    .Cast<TEntity>()
                    .ToList();
            }
            else
            {
                return Entity.ToList();
            }
        }

        public IQueryable<TEntity> GetBySpecification(Specification<TEntity> spec)
        {
            var query = Entity.AsQueryable();

            query = spec.IncludeEntities(query);

            return query.Where(spec.ToExpression());
        }
    }
}
