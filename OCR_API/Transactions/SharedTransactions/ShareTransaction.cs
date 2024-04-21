using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.Repositories;

namespace OCR_API.Transactions.SharedTransactions
{
    public abstract class ShareTransaction
    {
        protected readonly IRepository<Shared> repository;
        protected readonly int shareUserId;
        protected readonly int objectId;
        protected readonly EShareMode shareMode;

        public ShareTransaction(IRepository<Shared> repository, int shareUserId, int objectId, EShareMode shareMode)
        {
            this.repository = repository;
            this.shareUserId = shareUserId;
            this.objectId = objectId;
            this.shareMode = shareMode;
        }

        public abstract void Execute();
    }
}
