using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteFileConfiguration : IEntityTypeConfiguration<NoteFile>
    {
        public void Configure(EntityTypeBuilder<NoteFile> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.UserId, "note_files_ibfk_1");
            builder.ToTable("note_files");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Path).HasColumnName("path").IsRequired();
            builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(e => e.HashedKey).HasColumnName("hashed_key").HasDefaultValueSql(null);

            builder.HasMany(d => d.BoundingBoxes)
                .WithOne(r => r.NoteFile)
                .HasForeignKey(d => d.NoteFileId)
                .HasConstraintName("bounding_box_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Note)
            .WithOne(r => r.NoteFile)
            .HasForeignKey<Note>(d => d.NoteFileId)
            .HasConstraintName("notes_ibfk_3");

            builder.HasOne(d => d.User)
                .WithMany(r => r.NoteFiles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("note_files_ibfk_1");
        }
    }
}
