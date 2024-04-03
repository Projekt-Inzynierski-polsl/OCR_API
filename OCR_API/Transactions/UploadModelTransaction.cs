using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions
{
    public class UploadModelTransaction : ITransaction
    {
        private readonly IRepository<UploadedModel> repository;
        private readonly int userId;
        private readonly string path;
        public UploadedModel model;

        public UploadModelTransaction(IRepository<UploadedModel> repository, int userId, string path)
        {
            this.repository = repository;
            this.userId = userId;
            this.path = path;
        }

        public void Execute()
        {
            int nextId = repository.Entity.Count() + 1;
            string folderPath = Path.Combine(path, nextId.ToString());
            UploadedModel modelToUpload = new() { UserId = userId, Path = folderPath, UploadTime = DateTime.Now };
            repository.Add(modelToUpload);
            model = modelToUpload;
        }
    }
}
