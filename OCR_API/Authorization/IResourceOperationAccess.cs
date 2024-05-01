using OCR_API.Entities;

namespace OCR_API.Authorization
{
    public interface IResourceOperationAccess
    {
        public int UserId { get; set; }
        public ICollection<Shared> SharedObjects { get; set; }
    }
}
