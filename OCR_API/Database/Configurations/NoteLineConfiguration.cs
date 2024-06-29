using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteLineConfiguration : IEntityTypeConfiguration<NoteLine>
    {
        public void Configure(EntityTypeBuilder<NoteLine> builder)
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("note_lines");
            builder.HasIndex(e => e.BoundingBoxId, "note_lines_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.BoundingBoxId).HasColumnName("bounding_box_id").IsRequired();
            builder.Property(e => e.Content).HasColumnName("content").IsRequired();
            builder.Property(e => e.LeftX).HasColumnName("left_x").IsRequired();
            builder.Property(e => e.LeftY).HasColumnName("left_y").IsRequired();
            builder.Property(e => e.RightX).HasColumnName("right_x").IsRequired();
            builder.Property(e => e.RightY).HasColumnName("right_y").IsRequired();

            builder.HasOne(d => d.BoundingBox)
                .WithMany(r => r.Lines)
                .HasForeignKey(d => d.BoundingBoxId)
                .HasConstraintName("note_lines_ibfk_1");
        }
    }
}