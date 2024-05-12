using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteFileTransactions
{
    public class UploadNoteFileTransaction : ITransaction
    {
        private readonly IRepository<NoteFile> repository;
        private readonly string path;
        public NoteFile FileToUpload;

        public UploadNoteFileTransaction(IRepository<NoteFile> repository, string path, NoteFile fileToUpload)
        {
            this.repository = repository;
            this.path = path;
            this.FileToUpload = fileToUpload;
        }

        public void Execute()
        {
            int nextId = repository.Entity.Count() + 1;
            string filePath = Path.Combine(path, nextId.ToString());
            FileToUpload.Path = filePath;
            repository.Add(FileToUpload);
        }
    }
}
