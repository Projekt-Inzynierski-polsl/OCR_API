using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteLineWordConfiguration : IEntityTypeConfiguration<NoteLineWord>
    {
        public void Configure(EntityTypeBuilder<NoteLineWord> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.ToTable("note_line_words");
            builder.HasIndex(e => e.LineId, "note_line_words_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.LineId).HasColumnName("line_id").IsRequired();
            builder.Property(e => e.Coordinates)
               .HasDefaultValueSql("'{}'")
               .HasColumnType("json")
               .HasColumnName("coordinates");

            builder.HasOne(d => d.NoteLine)
                .WithMany(r => r.Words)
                .HasForeignKey(d => d.LineId)
                .HasConstraintName("note_line_words_ibfk_1");

        }
    }
}
