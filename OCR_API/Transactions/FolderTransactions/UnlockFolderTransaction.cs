using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;

namespace OCR_API.Transactions.FolderTransactions
{
    public class UnlockFolderTransaction : ITransaction
    {
        private readonly Folder folderToUnlock;


        public UnlockFolderTransaction(Folder folderToUnlock)
        {
            this.folderToUnlock = folderToUnlock;

        }

        public void Execute()
        {
            folderToUnlock.PasswordHash = null;
        }
    }
}
