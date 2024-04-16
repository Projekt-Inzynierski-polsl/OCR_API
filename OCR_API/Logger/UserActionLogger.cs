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
            string description = GetActionDescription(action, userId, objectId);
            var userLog = new UserLog() { ActionId = (int)action, Description = description, UserId = userId, LogTime = time };
            repository.Add(userLog);
        }

        private string GetActionDescription(EUserAction action, int userId, int? objectId)
        {
            switch (action)
            {
                case EUserAction.Registration:
                    return $"Registered user ({userId})";
                case EUserAction.Login:
                    return $"Login user ({userId})";
                case EUserAction.RefreshToken:
                    return $"Token refreshed by user ({userId})";
                case EUserAction.LogoutUser:
                    return $"Error ({objectId}) deleted by user ({userId})";
                case EUserAction.CreateFolder:
                    return $"Folder ({objectId}) added by user ({userId})";
                case EUserAction.DeleteFolder:
                    return $"Folder({objectId}) deleted by user ({userId})";
                case EUserAction.UpdateFolder:
                    return $"Folder ({objectId}) updated by user ({userId})";
                case EUserAction.LockFolder:
                    return $"Folder ({objectId}) locked by user ({userId})";
                case EUserAction.UnlockFolder:
                    return $"Folder ({objectId}) unlocked by user ({userId})";
                case EUserAction.CreateNote:
                    return $"Note ({objectId}) created by user ({userId})";
                case EUserAction.DeleteNote:
                    return $"Note ({objectId}) deleted by user ({userId})";
                case EUserAction.UpdateNote:
                    return $"Note ({objectId}) updated by user ({userId})";
                case EUserAction.ChangeNoteFolder:
                    return $"Note ({objectId}) folder changed by user ({userId})";
                case EUserAction.UpdateNoteCategories:
                    return $"Note ({objectId}) categories updated by user ({userId})";
                case EUserAction.ReportError:
                    return $"Error ({objectId}) reported by user ({userId})";
                case EUserAction.UpdateUser:
                    return $"User ({objectId}) details edited by user ({userId})";
                case EUserAction.DeleteUser:
                    return $"User ({objectId}) deleted by user ({userId})";
                case EUserAction.DownloadErrors:
                    return $"Errors downloaded by user ({userId})";
                case EUserAction.ClearErrorTable:
                    return $"Error table cleared by user ({userId})";
                case EUserAction.AddCategory:
                    return $"Category ({objectId}) added by user ({userId})";
                case EUserAction.DeleteCategory:
                    return $"Category ({objectId}) deleted by user ({userId})";
                case EUserAction.UpdateCategory:
                    return $"Category ({objectId}) updated by user ({userId})";
                default:
                     return "Unknown action";
            }
        }
    }
}
