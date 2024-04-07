using OCR_API.Repositories;

namespace OCR_API.Transactions
{
    public class TruncateTableTransaction<TEntity> : ITransaction where TEntity : class
    {
        private readonly IRepository<TEntity> repository;

        public TruncateTableTransaction(IRepository<TEntity> repository)
        {
            this.repository = repository;
        }

        public void Execute()
        {
            repository.Entity.RemoveRange(repository.Entity);
        }
    }
}
