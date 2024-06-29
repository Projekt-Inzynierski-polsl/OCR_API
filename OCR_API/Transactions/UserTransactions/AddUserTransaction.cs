using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.UserTransactions
{
    public class AddUserTransaction : ITransaction
    {
        private readonly IRepository<User> repository;
        public User userToAdd;

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