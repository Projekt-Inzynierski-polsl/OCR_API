using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteFileTransactions
{
    public class UploadNoteFileTransaction : ITransaction
    {
        private readonly IRepository<NoteFile> repository;
        public NoteFile FileToUpload;

        public UploadNoteFileTransaction(IRepository<NoteFile> repository, NoteFile fileToUpload)
        {
            this.repository = repository;
            this.FileToUpload = fileToUpload;
        }

        public void Execute()
        {
            repository.Add(FileToUpload);
        }
    }
}