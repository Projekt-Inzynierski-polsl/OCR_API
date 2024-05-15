using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions.NoteWordErrorTransactions
{
    public class AcceptErrorTransaction : ITransaction
    {
        private readonly NoteWordError errorToAccept;

        public AcceptErrorTransaction(NoteWordError errorToAccept)
        {
            this.errorToAccept = errorToAccept;
        }

        public void Execute()
        {
            errorToAccept.IsAccepted = true;
        }
    }
}
