using OCR_API.Entities;

namespace OCR_API.Transactions.NoteCategoriesTransactions
{
    public class AddNoteCategoryTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly int userId;
        private readonly string name;
        private readonly string? hexColor;
        public NoteCategory NoteCategoryToAdd;

        public AddNoteCategoryTransaction(IUnitOfWork unitOfWork, int userId, string name, string? hexColor)
        {
            this.unitOfWork = unitOfWork;
            this.userId = userId;
            this.name = name;
            this.hexColor = hexColor;
        }

        public void Execute()
        {
            NoteCategoryToAdd = new NoteCategory() { Name = name, HexColor = hexColor };
            NoteCategoryToAdd.UserId = userId;
            unitOfWork.NoteCategories.Add(NoteCategoryToAdd);
        }
    }
}