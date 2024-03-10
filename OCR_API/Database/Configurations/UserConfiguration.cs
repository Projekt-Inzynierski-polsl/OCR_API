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
            builder.ToTable("users");
            builder.HasIndex(e => e.RoleId, "user_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Email).HasColumnType("text").HasColumnName("email").IsRequired();
            builder.Property(e => e.Nick).HasColumnType("text").HasColumnName("nick").IsRequired();
            builder.Property(e => e.PasswordHash).HasColumnType("text").HasColumnName("password_hash").IsRequired();
            builder.Property(e => e.RoleId).HasColumnName("role_id");
            builder.Property(e => e.Test).IsRequired();

            builder.HasOne(d => d.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(d => d.RoleId)
            .HasConstraintName("user_ibfk_1");
        }
    }
}
