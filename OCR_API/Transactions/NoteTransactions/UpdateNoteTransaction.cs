using OCR_API.Entities;

namespace OCR_API.Transactions.NoteTransactions
{
    public class UpdateNoteTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly int userId;
        private readonly Note noteToUpdate;
        private readonly string newName;
        private readonly bool isPrivate;

        public UpdateNoteTransaction(IUnitOfWork unitOfWork, int userId, Note noteToUpdate, string newName, bool isPrivate)
        {
            this.unitOfWork = unitOfWork;
            this.userId = userId;
            this.noteToUpdate = noteToUpdate;
            this.newName = newName;
            this.isPrivate = isPrivate;
        }
        public void Execute()
        {
            noteToUpdate.Name = newName;
            noteToUpdate.IsPrivate = isPrivate;
        }
    }
}
