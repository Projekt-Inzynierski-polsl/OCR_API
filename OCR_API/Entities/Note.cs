﻿using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace OCR_API.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FolderId { get; set; }
        public int NoteFileId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsPrivate { get; set; }

        public virtual User User { get; set; }
        public virtual Folder Folder { get; set; }
        public virtual NoteFile NoteFile { get; set; }
        public virtual ICollection<NoteCategory> Categories { get; set; } = new List<NoteCategory>();
        public virtual ICollection<NoteWorldError> WorldErrors { get; set; } = new List<NoteWorldError>();

    }
}
