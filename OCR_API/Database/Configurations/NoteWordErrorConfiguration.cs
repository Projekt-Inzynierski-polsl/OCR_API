using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteWordErrorConfiguration : IEntityTypeConfiguration<NoteWordError>
    {
        public void Configure(EntityTypeBuilder<NoteWordError> builder)
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("note_word_errors");
            builder.HasIndex(e => e.FileId, "note_word_errors_ibfk_1");
            builder.HasIndex(e => e.UserId, "note_word_errors_ibfk_2");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.FileId).HasColumnName("file_id").IsRequired();
            builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(e => e.CorrectContent).HasColumnName("correct_content");
        
            builder.HasOne(d => d.File)
                .WithMany(r => r.Errors)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("note_word_errors_ibfk_1");

            builder.HasOne(d => d.User)
                .WithMany(r => r.Errors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("note_word_errors_ibfk_2");

        }
    }
}
