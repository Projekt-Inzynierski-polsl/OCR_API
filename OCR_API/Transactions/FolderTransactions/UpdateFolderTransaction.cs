using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.FolderTransactions
{
    public class UpdateFolderTransaction : ITransaction
    {
        private readonly Folder folderToUpdate;
        private readonly string newName;
        private readonly string newIconPath;

        public UpdateFolderTransaction(Folder folderToUpdate, string newName, string newIconPath)
        {
            this.folderToUpdate = folderToUpdate;
            this.newName = newName;
            this.newIconPath = newIconPath;
        }
        public void Execute()
        {
            folderToUpdate.Name = newName;
            folderToUpdate.IconPath = newIconPath;
        }
    }
}
