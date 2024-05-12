using Newtonsoft.Json;
using OCR_API.Entities.Inherits;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCR_API.Entities
{
    public class NoteFile : Entity, IHasUserId, IHasHashedKey
    {
        public int UserId { get; set; }
        public string Path { get; set; }
        public string? HashedKey { get; set; } = null;
        public virtual ICollection<BoundingBox> BoundingBoxes { get; set; } = new List<BoundingBox>();
        public virtual Note Note { get; set; }
        public virtual User User { get; set; }
    }
}
