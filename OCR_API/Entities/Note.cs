﻿using OCR_API.Authorization;
using OCR_API.Entities.Inherits;

namespace OCR_API.Entities
{
    public class Note : Entity, IHasUserId, IResourceOperationAccess
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int? FolderId { get; set; } = null;
        public int NoteFileId { get; set; }
        public string Content { get; set; }

        public virtual User User { get; set; }
        public virtual Folder? Folder { get; set; } = null;
        public virtual NoteFile NoteFile { get; set; }
        public virtual ICollection<NoteCategory> Categories { get; set; } = new List<NoteCategory>();
        public virtual ICollection<Shared> SharedObjects { get; set; } = new List<Shared>();
    }
}