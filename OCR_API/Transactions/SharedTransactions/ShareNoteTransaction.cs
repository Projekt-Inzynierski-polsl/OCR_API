using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Transactions.SharedTransactions;

namespace OCR_API.Transactions.FolderTransactions
{
    public class ShareNoteTransaction : ShareTransaction, ITransaction
    {
        public ShareNoteTransaction(IRepository<Shared> repository, int shareUserId, int folderId, EShareMode shareMode)
            : base(repository, shareUserId, folderId, shareMode)
        {
        }
        public override void Execute()
        {
            Shared sharedToAdd = new() { NoteId = objectId, UserId = shareUserId, ModeId = (int)shareMode };
            repository.Add(sharedToAdd);
        }
    }
}
