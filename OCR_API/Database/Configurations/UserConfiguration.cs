using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;
using System.Reflection.Emit;

namespace OCR_API.Database.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("users");
            builder.HasIndex(e => e.RoleId, "user_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Email).HasColumnName("email").IsRequired();
            builder.Property(e => e.Nickname).HasColumnName("nick").IsRequired();
            builder.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            builder.Property(e => e.RoleId).HasColumnName("role_id");

            builder.HasOne(d => d.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user_ibfk_1");

            builder.HasMany(d => d.Folders)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("folder_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Notes)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notes_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.UploadedModels)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("uploaded_models_ibfk_1");


            builder.HasMany(d => d.Logs)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_logs_ibfk_1")
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.NoteCategories)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("note_category_list_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.BlackListedTokens)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("black_listed_tokens_ibfk_1")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Errors)
                .WithOne(r => r.User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("note_word_errors_ibfk_2")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.SharedObjects)
                .WithOne(u => u.User)
                .HasForeignKey(user => user.UserId)
                .HasConstraintName("shared_objects_ibfk_1");
        }
    }
}
