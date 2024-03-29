using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Repositories;

namespace OCR_API.Transactions.UserTransactions
{
    public class UpdateUserTransaction : ITransaction
    {
        private readonly IRepository<User> userRepository;
        private readonly int userId;
        private readonly User updatedUser;

        public UpdateUserTransaction(IRepository<User> userRepository, int userId, User updatedUser)
        {
            this.userRepository = userRepository;
            this.userId = userId;
            this.updatedUser = updatedUser;
        }
        public void Execute()
        {
            User userToUpdate = userRepository.GetById(userId);
            userToUpdate.Email = updatedUser.Email;
            userToUpdate.Nickname = updatedUser.Nickname;
            userToUpdate.PasswordHash = updatedUser.PasswordHash;
            userToUpdate.RoleId = updatedUser.RoleId;

        }
    }
}
