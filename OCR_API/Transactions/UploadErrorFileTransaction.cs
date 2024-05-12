using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions
{
    public class UploadErrorFileTransaction : ITransaction
    {
        private readonly IRepository<ErrorCutFile> repository;
        private readonly string path;
        public ErrorCutFile FileToUpload;

        public UploadErrorFileTransaction(IRepository<ErrorCutFile> repository, string path)
        {
            this.repository = repository;
            this.path = path;
        }

        public void Execute()
        {
            FileToUpload = new ErrorCutFile();
            int nextId = repository.Entity.Count() + 1;
            string filePath = Path.Combine(path, nextId.ToString());
            FileToUpload.Path = filePath;
            repository.Add(FileToUpload);
        }
    }
}
