using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteLineConfiguration : IEntityTypeConfiguration<NoteLine> 
    { 
        public void Configure(EntityTypeBuilder<NoteLine> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.ToTable("note_lines");
            builder.HasIndex(e => e.BoundingBoxId, "note_lines_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.BoundingBoxId).HasColumnName("path").IsRequired();
            builder.Property(e => e.Content).HasColumnName("content");
            builder.Property(e => e.Coordinates)
               .HasDefaultValueSql("'{}'")
               .HasColumnType("json")
               .HasColumnName("coordinates");

            builder.HasOne(d => d.BoundingBox)
                .WithMany(r => r.Lines)
                .HasForeignKey(d => d.BoundingBoxId)
                .HasConstraintName("note_lines_ibfk_1");

            builder.HasMany(d => d.WorldErrors)
                .WithOne(r => r.Line)
                .HasForeignKey(d => d.LineId)
                .HasConstraintName("note_world_errors_1");
        }
    }
}
