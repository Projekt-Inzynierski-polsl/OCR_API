using OCR_API.Entities;

namespace OCR_API.Logger
{
    public class UserActionLogger
    {
        private readonly IUnitOfWork unitOfWork;

        public UserActionLogger(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void Log(EUserAction action, int userId, DateTime time, int? objectId = null)
        {
            var userLog = new UserLog() { ActionId = (int)action, UserId = userId, LogTime = time, ObjectId = objectId };
            unitOfWork.UserLogs.Add(userLog);
            unitOfWork.Commit();
        }
    }
}