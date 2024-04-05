using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteCategoriesTransactions
{
    public class DeleteNoteCategoryTransaction : ITransaction
    {
        private readonly IRepository<NoteCategory> repository;
        private readonly int noteCategoryId;

        public DeleteNoteCategoryTransaction(IRepository<NoteCategory> repository, int noteCategoryId)
        {
            this.repository = repository;
            this.noteCategoryId = noteCategoryId;
        }

        public void Execute()
        {
            repository.Remove(noteCategoryId);
        }
    }
}
