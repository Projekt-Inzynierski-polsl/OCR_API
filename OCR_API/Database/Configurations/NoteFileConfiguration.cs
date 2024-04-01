using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteFileConfiguration : IEntityTypeConfiguration<NoteFile>
    {
        public void Configure(EntityTypeBuilder<NoteFile> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.ToTable("note_files");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Path).HasColumnName("path").IsRequired();

            builder.HasMany(d => d.BoundingBoxes)
                .WithOne(r => r.NoteFile)
                .HasForeignKey(d => d.NoteFileId)
                .HasConstraintName("bounding_box_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Notes)
            .WithOne(r => r.NoteFile)
            .HasForeignKey(d => d.NoteFileId)
            .HasConstraintName("notes_ibfk_3");
        }
    }
}
