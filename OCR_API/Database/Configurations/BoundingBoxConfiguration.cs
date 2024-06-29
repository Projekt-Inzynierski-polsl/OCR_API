using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class BoundingBoxConfiguration : IEntityTypeConfiguration<BoundingBox>
    {
        public void Configure(EntityTypeBuilder<BoundingBox> builder)
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("bounding_boxes");
            builder.HasIndex(e => e.NoteFileId, "bounding_box_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.NoteFileId).HasColumnName("file_id").IsRequired();
            builder.Property(e => e.LeftX).HasColumnName("left_x").IsRequired();
            builder.Property(e => e.LeftY).HasColumnName("left_y").IsRequired();
            builder.Property(e => e.RightX).HasColumnName("right_x").IsRequired();
            builder.Property(e => e.RightY).HasColumnName("right_y").IsRequired();

            builder.HasOne(d => d.NoteFile)
                .WithMany(r => r.BoundingBoxes)
                .HasForeignKey(d => d.NoteFileId)
                .HasConstraintName("bounding_box_ibfk_1");

            builder.HasMany(d => d.Lines)
                .WithOne(r => r.BoundingBox)
                .HasForeignKey(d => d.BoundingBoxId)
                .HasConstraintName("line_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}