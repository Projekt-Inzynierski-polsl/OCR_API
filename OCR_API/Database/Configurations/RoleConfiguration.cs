using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("roles");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired();
            builder.HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(user => user.RoleId)
                .HasConstraintName("user_ibfk_1");
        }
    }
}