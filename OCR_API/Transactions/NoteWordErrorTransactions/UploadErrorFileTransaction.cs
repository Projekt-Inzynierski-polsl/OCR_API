using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteWordErrorTransactions
{
    public class UploadErrorFileTransaction : ITransaction
    {
        private readonly IRepository<ErrorCutFile> repository;
        public ErrorCutFile FileToUpload;

        public UploadErrorFileTransaction(IRepository<ErrorCutFile> repository)
        {
            this.repository = repository;
        }

        public void Execute()
        {
            FileToUpload = new ErrorCutFile() { Path = "" };
            repository.Add(FileToUpload);
        }
    }
}
