using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.FolderTransactions
{
    public class DeleteFolderTransaction : ITransaction
    {
        private readonly IRepository<Folder> repository;
        private readonly int folderId;

        public DeleteFolderTransaction(IRepository<Folder> repository, int folderId)
        {
            this.repository = repository;
            this.folderId = folderId;
        }

        public void Execute()
        {
            repository.Remove(folderId);
        }
    }

}
