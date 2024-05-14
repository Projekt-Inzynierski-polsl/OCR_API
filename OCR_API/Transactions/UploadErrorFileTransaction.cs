using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions
{
    public class UploadErrorFileTransaction : ITransaction
    {
        private readonly IRepository<ErrorCutFile> repository;
        private readonly string path;
        private readonly string fileExtension;
        public ErrorCutFile FileToUpload;

        public UploadErrorFileTransaction(IRepository<ErrorCutFile> repository, string path, string fileExtension)
        {
            this.repository = repository;
            this.path = path;
            this.fileExtension = fileExtension;
        }

        public void Execute()
        {
            FileToUpload = new ErrorCutFile();
            int nextId = repository.Entity.Count() + 1;
            string filePath = Path.Combine(path, nextId.ToString() + fileExtension);
            FileToUpload.Path = filePath;
            repository.Add(FileToUpload);
        }
    }
}
