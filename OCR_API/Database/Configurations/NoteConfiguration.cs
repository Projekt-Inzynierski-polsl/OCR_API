﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("notes");
            builder.HasIndex(e => e.UserId, "notes_ibfk_1");
            builder.HasIndex(e => e.FolderId, "notes_ibfk_2");
            builder.HasIndex(e => e.NoteFileId, "notes_ibfk_3");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(e => e.FolderId).HasColumnName("folder_id").HasDefaultValueSql(null);
            builder.Property(e => e.NoteFileId).HasColumnName("file_id").IsRequired();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired();
            builder.Property(e => e.Content).HasColumnName("content");

            builder.HasOne(d => d.User)
                .WithMany(r => r.Notes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notes_ibfk_1");

            builder.HasOne(d => d.Folder)
                .WithMany(r => r.Notes)
                .HasForeignKey(d => d.FolderId)
                .HasConstraintName("notes_ibfk_2")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(d => d.NoteFile)
                .WithOne(r => r.Note)
                .HasForeignKey<Note>(d => d.NoteFileId)
                .HasConstraintName("notes_ibfk_3")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(n => n.Categories)
                .WithMany(c => c.Notes)
                .UsingEntity(j => j.ToTable("notes_category_map"));

            builder.HasMany(f => f.SharedObjects)
                .WithOne(u => u.Note)
                .HasForeignKey(f => f.NoteId)
                .HasConstraintName("shared_objects_ibfk_3")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}