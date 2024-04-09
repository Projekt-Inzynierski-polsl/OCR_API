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
                case EUserAction.AddFolder:
                    return $"Folder ({objectId}) added by user ({userId})";
                case EUserAction.RemoveFolder:
                    return $"Folder({objectId}) removed by user ({userId})";
                case EUserAction.EditFolder:
                    return $"Folder ({objectId}) edited by user ({userId})";
                case EUserAction.AddNote:
                    return $"Note ({objectId}) added by user ({userId})";
                case EUserAction.RemoveNote:
                    return $"Note ({objectId}) removed by user ({userId})";
                case EUserAction.EditNote:
                    return $"Note ({objectId}) edited by user ({userId})";
                case EUserAction.ChangeNoteFolder:
                    return $"Note ({objectId}) folder changed by user ({userId})";
                case EUserAction.AddCategoryToNote:
                    return $"Note ({objectId}) categories updated by user ({userId})";
                case EUserAction.ReportError:
                    return $"Error ({objectId}) reported by user ({userId})";
                case EUserAction.EditUser:
                    return $"User ({objectId}) details edited by user ({userId})";
                case EUserAction.LogoutUser:
                    return $"User ({userId}) logged out";
                case EUserAction.DeleteError:
                    return $"Error ({objectId}) deleted by user ({userId})";
                case EUserAction.DownloadErrors:
                    return $"Errors downloaded by user ({userId})";
                case EUserAction.ClearErrorTable:
                    return $"Error table cleared by user ({userId})";
                case EUserAction.AddCategory:
                    return $"Category ({objectId}) added by user ({userId})";
                case EUserAction.RemoveCategory:
                    return $"Category ({objectId}) removed by user ({userId})";
                case EUserAction.UpdateCategory:
                    return $"Category ({objectId}) updated by user ({userId})";
                default:
                     return "Unknown action";
            }
        }
    }
}
