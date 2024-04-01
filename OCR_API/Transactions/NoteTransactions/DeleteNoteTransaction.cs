using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteTransactions
{
    public class DeleteNoteTransaction : ITransaction
    {
        private readonly IRepository<Note> repository;
        private readonly int noteId;

        public DeleteNoteTransaction(IRepository<Note> repository, int noteId)
        {
            this.repository = repository;
            this.noteId = noteId;
        }

        public void Execute()
        {
            repository.Remove(noteId);
        }
    }

}
