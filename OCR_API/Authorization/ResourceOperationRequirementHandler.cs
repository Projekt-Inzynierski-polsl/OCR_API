using OCR_API.Enums;

namespace OCR_API.Authorization
{
    public class ResourceOperationAccess
    {
        public static bool IsShared(IResourceOperationAccess resource, int userId) => resource.SharedObjects.Any(f => f.UserId == userId) || resource.SharedObjects.Any(f => f.UserId == null);

        public static bool CanEdit(IResourceOperationAccess resource, int userId) => resource.UserId == userId || resource.SharedObjects.FirstOrDefault(f => f.UserId == null).ModeId == (int)EShareMode.Edit || resource.SharedObjects.FirstOrDefault(f => f.UserId == userId).ModeId == (int)EShareMode.Edit;
    }
}