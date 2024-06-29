using OCR_API.Entities;

namespace OCR_API.Transactions.NoteCategoriesTransactions
{
    public class UpdateNoteCategoryTransaction : ITransaction
    {
        private readonly NoteCategory noteCategoryToUpdate;
        private readonly string? newName;
        private readonly string? newColor;

        public UpdateNoteCategoryTransaction(NoteCategory notecategoryToUpdate, string? newName, string? newColor)
        {
            this.noteCategoryToUpdate = notecategoryToUpdate;
            this.newName = newName;
            this.newColor = newColor;
        }

        public void Execute()
        {
            if (newName is not null)
            {
                noteCategoryToUpdate.Name = newName;
            }
            if (newColor is not null)
            {
                noteCategoryToUpdate.HexColor = newColor;
            }
        }
    }
}