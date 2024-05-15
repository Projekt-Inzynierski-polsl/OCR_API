using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteWordErrorTransactions
{
    public class AddErrorTransaction : ITransaction
    {
        private readonly IRepository<NoteWordError> repository;
        public readonly NoteWordError ErrorToAdd;

        public AddErrorTransaction(IRepository<NoteWordError> repository, NoteWordError errorToAdd)
        {
            this.repository = repository;
            this.ErrorToAdd = errorToAdd;
        }

        public void Execute()
        {
            repository.Add(ErrorToAdd);
        }
    }
}
