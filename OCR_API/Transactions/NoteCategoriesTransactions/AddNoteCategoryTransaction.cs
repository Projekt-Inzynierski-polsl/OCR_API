using OCR_API.Entities;
using OCR_API.Exceptions;

namespace OCR_API.Transactions.NoteCategoriesTransactions
{
    public class AddNoteCategoryTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly int userId;
        private readonly string name;
        public NoteCategory NoteCategoryToAdd;

        public AddNoteCategoryTransaction(IUnitOfWork unitOfWork, int userId, string name)
        {
            this.unitOfWork = unitOfWork;
            this.userId = userId;
            this.name = name;
        }

        public void Execute()
        {
            NoteCategoryToAdd = new NoteCategory() { Name = name };
            NoteCategoryToAdd.UserId = userId;
            unitOfWork.NoteCategories.Add(NoteCategoryToAdd);
        }
    }
}
