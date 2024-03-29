using Newtonsoft.Json.Linq;
using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Transactions
{
    public class AddBlackListedTokenTransaction : ITransaction
    {
        private readonly IRepository<BlackListToken> blackListedTokensRepository;
        private readonly int userId;
        private readonly string jwtToken;

        public AddBlackListedTokenTransaction(IRepository<BlackListToken> blackListedTokensRepository, int userId, string token)
        {
            this.blackListedTokensRepository = blackListedTokensRepository;
            this.userId = userId;
            this.jwtToken = token;
        }

        public void Execute()
        {
            BlackListToken tokenToAdd = new BlackListToken() { Token = jwtToken, UserId = userId };
            blackListedTokensRepository.Add(tokenToAdd);
        }
    }
}
