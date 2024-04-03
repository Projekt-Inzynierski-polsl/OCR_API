using OCR_API.Entities;

namespace OCR_API.Transactions.NoteTransactions
{
    public class UpdateNoteTransaction : ITransaction
    {
        private readonly Note noteToUpdate;
        private readonly string newName;
        private readonly string newContent;
        private readonly bool isPrivate;

        public UpdateNoteTransaction(Note noteToUpdate, string newName, string newContent, bool isPrivate)
        {
            this.noteToUpdate = noteToUpdate;
            this.newName = newName;
            this.newContent = newContent;
            this.isPrivate = isPrivate;
        }
        public void Execute()
        {
            noteToUpdate.Name = newName;
            noteToUpdate.Content = newContent;
            noteToUpdate.IsPrivate = isPrivate;
        }
    }
}
