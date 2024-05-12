﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
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
            builder.Property(e => e.Coordinates)
                   .HasColumnType("nvarchar(max)")
                   .HasColumnName("coordinates")
                   .HasDefaultValue("{}");

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
