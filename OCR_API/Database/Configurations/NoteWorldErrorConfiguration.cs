using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteWorldErrorConfiguration : IEntityTypeConfiguration<NoteWorldError>
    {
        public void Configure(EntityTypeBuilder<NoteWorldError> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.ToTable("note_world_errors");
            builder.HasIndex(e => e.LineId, "note_world_errors_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.LineId).HasColumnName("line_id").IsRequired();
            builder.Property(e => e.CorrectContent).HasColumnName("correct_content");
        
            builder.HasOne(d => d.Line)
                .WithMany(r => r.WorldErrors)
                .HasForeignKey(d => d.LineId)
                .HasConstraintName("note_world_errors_ibfk_1");

        }
    }
}
