using OCR_API.Entities;
using OCR_API.Exceptions;

namespace OCR_API.Transactions.NoteTransactions
{
    public class AddNoteTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly int userId;
        public readonly Note NoteToAdd;

        public AddNoteTransaction(IUnitOfWork unitOfWork, int userId, Note noteToAdd)
        {
            this.unitOfWork = unitOfWork;
            this.userId = userId;
            this.NoteToAdd = noteToAdd;
        }

        public void Execute()
        {
            NoteToAdd.UserId = userId;
            if (NoteToAdd.FolderId == null)
            {
                unitOfWork.Notes.Add(NoteToAdd);
            }
            else
            {
                Folder folder = unitOfWork.Folders.GetById((int)NoteToAdd.FolderId);
                if (folder is not null && folder.UserId == userId)
                {
                    unitOfWork.Notes.Add(NoteToAdd);
                }
                else
                {
                    throw new BadRequestException("Wrong folder id.");
                }
            }
        }
    }
}