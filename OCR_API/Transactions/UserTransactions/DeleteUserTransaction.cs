using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.UserTransactions
{
    public class DeleteUserTransaction : ITransaction
    {
        private readonly IRepository<User> repository;
        private readonly int userId;

        public DeleteUserTransaction(IRepository<User> repository, int userId)
        {
            this.repository = repository;
            this.userId = userId;
        }

        public void Execute()
        {
            repository.Remove(userId);
        }
    }
}
