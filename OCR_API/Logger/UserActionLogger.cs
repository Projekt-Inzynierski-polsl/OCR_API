using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.Logger
{
    public class UserActionLogger
    {
        private readonly IRepository<UserLog> repository;

        public UserActionLogger(IRepository<UserLog> repository)
        {
            this.repository = repository;
        }
        public void Log(EUserAction action, int userId, DateTime time, int? objectId = null)
        {
            var userLog = new UserLog() { ActionId = (int)action, UserId = userId, LogTime = time };
            repository.Add(userLog);
        }
    }
}
