using OCR_API.Entities;
using OCR_API.Repositories;
using System.Transactions;

namespace OCR_API.Transactions.UserTransactions
{
    public class AddUserTransaction : ITransaction
    {
        private readonly IRepository<User> repository;
        private readonly User userToAdd;

        public AddUserTransaction(IRepository<User> repository, User userToAdd)
        {
            this.repository = repository;
            this.userToAdd = userToAdd;
        }

        public void Execute()
        {
            repository.Add(userToAdd);
        }
    }
}
