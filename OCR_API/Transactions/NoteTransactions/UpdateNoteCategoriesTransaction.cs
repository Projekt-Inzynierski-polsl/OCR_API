using OCR_API.Entities;
using OCR_API.Exceptions;

namespace OCR_API.Transactions.NoteTransactions
{
    public class UpdateNoteCategoriesTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Note noteToUpdate;
        private readonly int userId;
        private readonly int[] categoriesIds;
        private readonly int folderId;

        public UpdateNoteCategoriesTransaction(IUnitOfWork unitOfWork, Note noteToUpdate, int userId, int[] categoriesIds)
        {
            this.unitOfWork = unitOfWork;
            this.noteToUpdate = noteToUpdate;
            this.userId = userId;
            this.categoriesIds = categoriesIds;
        }
        public void Execute()
        {
            List<NoteCategory> categories = new List<NoteCategory>();
            foreach (int id in categoriesIds)
            {
                NoteCategory noteCategory = unitOfWork.NoteCategories.GetById(id);
                if (noteCategory is not null && noteCategory.UserId == userId)
                {
                    categories.Add(noteCategory);
                }
            }
            noteToUpdate.Categories = categories;
        }
    }
}
