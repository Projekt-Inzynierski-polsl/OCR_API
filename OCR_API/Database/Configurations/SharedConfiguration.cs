using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class SharedConfiguration : IEntityTypeConfiguration<Shared>
    {
        public void Configure(EntityTypeBuilder<Shared> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.UserId, "shared_objects_ibfk_1");
            builder.HasIndex(e => e.FolderId, "shared_objects_ibfk_2");
            builder.HasIndex(e => e.NoteId, "shared_objects_ibfk_3");
            builder.HasIndex(e => e.ModeId, "shared_objects_ibfk_4");

            builder.ToTable("shared_objects");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.UserId).HasColumnName("user_id").HasDefaultValueSql(null);

            builder.Property(e => e.FolderId).HasColumnName("folder_id").HasDefaultValueSql(null);
            builder.Property(e => e.NoteId).HasColumnName("note_id").HasDefaultValueSql(null);

            builder.Property(e => e.ModeId).HasColumnName("mode_id").IsRequired();

            builder.HasOne(f => f.User)
                .WithMany(u => u.SharedObjects)
                .HasForeignKey(user => user.UserId)
                .HasConstraintName("shared_objects_ibfk_1");

            builder.HasOne(f => f.Folder)
                .WithMany(u => u.SharedObjects)
                .HasForeignKey(f => f.FolderId)
                .HasConstraintName("shared_objects_ibfk_2");

            builder.HasOne(f => f.Note)
                .WithMany(u => u.SharedObjects)
                .HasForeignKey(f => f.NoteId)
                .HasConstraintName("shared_objects_ibfk_3")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.Mode)
                .WithMany(u => u.SharedObjects)
                .HasForeignKey(f => f.ModeId)
                .HasConstraintName("shared_objects_ibfk_4");
        }
    }
}