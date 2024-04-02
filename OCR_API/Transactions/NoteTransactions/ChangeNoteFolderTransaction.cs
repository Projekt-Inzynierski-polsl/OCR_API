using OCR_API.Entities;
using OCR_API.Exceptions;

namespace OCR_API.Transactions.NoteTransactions
{
    public class ChangeNoteFolderTransaction : ITransaction
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly int userId;
        private readonly Note noteToMove;
        private readonly int? folderId;

        public ChangeNoteFolderTransaction(IUnitOfWork unitOfWork, int userId, Note noteToMove, int? folderId)
        {
            this.unitOfWork = unitOfWork;
            this.userId = userId;
            this.noteToMove = noteToMove;
            this.folderId = folderId;
        }
        public void Execute()
        {
            if(folderId == null)
            {
                noteToMove.FolderId = 0;
            }
            else
            {
                Folder folder = unitOfWork.Folders.GetById((int)folderId);
                if (folder is not null && folder.UserId == userId)
                {
                    noteToMove.FolderId = folderId;
                }
                else
                {
                    throw new BadRequestException("Wrong folder id.");
                }
            }  
        }
    }
}
