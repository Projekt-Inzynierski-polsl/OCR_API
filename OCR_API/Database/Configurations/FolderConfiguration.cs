using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class FolderConfigurationConfiguration : IEntityTypeConfiguration<Folder>
    {
        public void Configure(EntityTypeBuilder<Folder> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.ToTable("folders");
            builder.HasIndex(e => e.UserId, "folder_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired();
            builder.Property(e => e.IconPath).HasColumnName("icon_path");
            builder.Property(e => e.PasswordHash).HasColumnName("password_hash");

            builder.HasOne(d => d.User)
                .WithMany(r => r.Folders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("folder_ibfk_1");

            builder.HasMany(d => d.Notes)
                .WithOne(r => r.Folder)
                .HasForeignKey(d => d.FolderId)
                .HasConstraintName("notes_ibfk_2");
        }
    }
}
