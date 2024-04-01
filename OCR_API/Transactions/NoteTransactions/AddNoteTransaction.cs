using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteTransactions
{
    public class AddNoteTransaction : ITransaction
    {
        private readonly IRepository<Note> repository;
        private readonly int userId;
        public readonly Note NoteToAdd;

        public AddNoteTransaction(IRepository<Note> repository, int userId, Note noteToAdd)
        {
            this.repository = repository;
            this.userId = userId;
            this.NoteToAdd = noteToAdd;
        }

        public void Execute()
        {
            NoteToAdd.UserId = userId;
            repository.Add(NoteToAdd);
        }
    }
}
