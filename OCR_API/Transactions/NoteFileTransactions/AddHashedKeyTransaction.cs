using OCR_API.Entities;
using OCR_API.Entities.Inherits;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteFileTransactions
{
    public class AddHashedKeyTransaction<T> : ITransaction where T : Entity
    {
        private readonly IRepository<T> repository;
        private readonly string hashedKey;
        private readonly int fileId;

        public AddHashedKeyTransaction(IRepository<T> repository, string hashedKey, int fileId)
        {
            this.repository = repository;
            this.hashedKey = hashedKey;
            this.fileId = fileId;
        }

        public void Execute()
        {
            var noteFile = repository.GetById(fileId);
            if (noteFile is IHasHashedKey file)
            {
                file.HashedKey = hashedKey;
            }
        }
    }
}