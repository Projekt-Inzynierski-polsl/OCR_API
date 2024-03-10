using OCR_API.Entities;
using OCR_API.Repositories;
using System.Transactions;

namespace OCR_API.Transactions
{
    public class AddUserTransaction : ITransaction
    {
        private readonly IRepository<User> userRepository;
        private readonly User userToAdd;

        public AddUserTransaction(IRepository<User> userRepository, User userToAdd)
        {
            this.userRepository = userRepository;
            this.userToAdd = userToAdd;
        }

        public void Execute()
        {
            userRepository.Add(userToAdd);
        }
    }
}
