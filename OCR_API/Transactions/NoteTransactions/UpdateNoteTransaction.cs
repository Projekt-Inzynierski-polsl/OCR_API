using OCR_API.Entities;

namespace OCR_API.Transactions.NoteTransactions
{
    public class UpdateNoteTransaction : ITransaction
    {
        private readonly Note noteToUpdate;
        private readonly string? newName;
        private readonly string? newContent;
        private readonly bool? isPrivate;

        public UpdateNoteTransaction(Note noteToUpdate, string? newName, string? newContent, bool? isPrivate)
        {
            this.noteToUpdate = noteToUpdate;
            this.newName = newName;
            this.newContent = newContent;
            this.isPrivate = isPrivate;
        }
        public void Execute()
        {
            if(newName is not null)
            {
                noteToUpdate.Name = newName;
            }
            if(newContent is not null)
            {
                noteToUpdate.Content = newContent;
            }
            if(isPrivate is not null)
            {
                noteToUpdate.IsPrivate = (bool)isPrivate;
            }
        }
    }
}
