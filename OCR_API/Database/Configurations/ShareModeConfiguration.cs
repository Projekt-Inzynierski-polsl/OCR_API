using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class ShareModeConfiguration : IEntityTypeConfiguration<ShareMode>
    {
        public void Configure(EntityTypeBuilder<ShareMode> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("share_modes");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired();
            builder.HasMany(r => r.SharedObjects)
                .WithOne(u => u.Mode)
                .HasForeignKey(user => user.ModeId)
                .HasConstraintName("shared_objects_ibfk_4");

        }
    }
}
