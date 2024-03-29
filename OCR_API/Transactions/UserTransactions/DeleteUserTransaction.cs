using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.UserTransactions
{
    public class DeleteUserTransaction : ITransaction
    {
        private readonly IRepository<User> userRepository;
        private readonly int userId;

        public DeleteUserTransaction(IRepository<User> userRepository, int userId)
        {
            this.userRepository = userRepository;
            this.userId = userId;
        }

        public void Execute()
        {
            userRepository.Remove(userId);
        }
    }
}
