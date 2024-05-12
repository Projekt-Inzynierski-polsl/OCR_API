using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class UserActionConfiguration : IEntityTypeConfiguration<UserAction>
    {
        public void Configure(EntityTypeBuilder<UserAction> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("user_actions");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired();

            builder.HasMany(r => r.Logs)
                .WithOne(u => u.Action)
                .HasForeignKey(user => user.ActionId)
                .HasConstraintName("user_actions_ibfk_1");

        }
    }
}
