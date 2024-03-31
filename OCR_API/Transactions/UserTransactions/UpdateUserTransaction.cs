using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Repositories;

namespace OCR_API.Transactions.UserTransactions
{
    public class UpdateUserTransaction : ITransaction
    {
        private readonly IRepository<User> repository;
        private readonly int userId;
        private readonly User updatedUser;

        public UpdateUserTransaction(IRepository<User> repository, int userId, User updatedUser)
        {
            this.repository = repository;
            this.userId = userId;
            this.updatedUser = updatedUser;
        }
        public void Execute()
        {
            User userToUpdate = repository.GetById(userId);
            userToUpdate.Email = updatedUser.Email;
            userToUpdate.Nickname = updatedUser.Nickname;
            userToUpdate.RoleId = updatedUser.RoleId;
            if(updatedUser.PasswordHash != null)
            {
                userToUpdate.PasswordHash = updatedUser.PasswordHash;
            }

        }
    }
}
