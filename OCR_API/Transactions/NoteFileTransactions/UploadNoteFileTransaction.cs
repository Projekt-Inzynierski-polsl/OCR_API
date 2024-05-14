using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteFileTransactions
{
    public class UploadNoteFileTransaction : ITransaction
    {
        private readonly IRepository<NoteFile> repository;
        private readonly string path;
        public NoteFile FileToUpload;
        private readonly string fileExtension;

        public UploadNoteFileTransaction(IRepository<NoteFile> repository, string path, NoteFile fileToUpload, string fileExtension)
        {
            this.repository = repository;
            this.path = path;
            this.FileToUpload = fileToUpload;
            this.fileExtension = fileExtension;
        }

        public void Execute()
        {
            int nextId = repository.Entity.Count() + 1;
            string filePath = Path.Combine(path, nextId.ToString() + fileExtension);
            FileToUpload.Path = filePath;
            repository.Add(FileToUpload);
        }
    }
}
