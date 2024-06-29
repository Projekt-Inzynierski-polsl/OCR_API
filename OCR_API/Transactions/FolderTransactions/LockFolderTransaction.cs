using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;

namespace OCR_API.Transactions.FolderTransactions
{
    public class LockFolderTransaction : ITransaction
    {
        private readonly Folder folderToLock;
        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly string password;

        public LockFolderTransaction(Folder folderToLock, IPasswordHasher<Folder> passwordHasher, string password)
        {
            this.folderToLock = folderToLock;
            this.passwordHasher = passwordHasher;
            this.password = password;
        }

        public void Execute()
        {
            var hashedPassword = passwordHasher.HashPassword(folderToLock, password);
            folderToLock.PasswordHash = hashedPassword;
        }
    }
}