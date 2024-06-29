using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions
{
    public class DeleteEntityTransaction<TEntity> : ITransaction where TEntity : Entity
    {
        private readonly IRepository<TEntity> repository;
        private readonly int id;

        public DeleteEntityTransaction(IRepository<TEntity> repository, int id)
        {
            this.repository = repository;
            this.id = id;
        }

        public void Execute()
        {
            repository.Remove(id);
        }
    }
}