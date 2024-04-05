using OCR_API.Entities;

namespace OCR_API.Transactions.NoteCategoriesTransactions
{
    public class UpdateNoteCategoryTransaction : ITransaction
    {
        private readonly NoteCategory noteCategoryToUpdate;
        private readonly string newName;

        public UpdateNoteCategoryTransaction(NoteCategory notecategoryToUpdate, string newName)
        {
            this.noteCategoryToUpdate = notecategoryToUpdate;
            this.newName = newName;
        }
        public void Execute()
        {
            noteCategoryToUpdate.Name = newName;
        }
    }
}
