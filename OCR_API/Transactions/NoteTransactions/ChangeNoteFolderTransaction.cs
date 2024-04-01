using OCR_API.Entities;
using OCR_API.Exceptions;

namespace OCR_API.Transactions.NoteTransactions
{
    public class ChangeNoteFolderTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly int userId;
        private readonly Note noteToMove;
        private readonly int folderId;

        public ChangeNoteFolderTransaction(IUnitOfWork unitOfWork, int userId, Note noteToMove, int folderId)
        {
            this.unitOfWork = unitOfWork;
            this.userId = userId;
            this.noteToMove = noteToMove;
            this.folderId = folderId;
        }
        public void Execute()
        {
            Folder folder = unitOfWork.Folders.GetById(folderId);
            if(folder is not null && folder.UserId == userId)
            {
                noteToMove.FolderId = folderId != 0 ? folderId : null;
            }
            else
            {
                throw new BadRequestException("Wrong folder id.");
            }
            
        }
    }
}
