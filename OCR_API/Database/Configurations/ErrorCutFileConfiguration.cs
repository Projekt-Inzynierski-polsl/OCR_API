﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class ErrorCutFileConfiguration : IEntityTypeConfiguration<ErrorCutFile>
    {
        public void Configure(EntityTypeBuilder<ErrorCutFile> builder)
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("error_cut_files");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Path).HasColumnName("path").IsRequired();
            builder.Property(e => e.HashedKey).HasColumnName("hashed_key").HasDefaultValueSql(null);

            builder.HasMany(d => d.Errors)
                .WithOne(r => r.File)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("note_word_errors_ibfk_1");
        }
    }
}