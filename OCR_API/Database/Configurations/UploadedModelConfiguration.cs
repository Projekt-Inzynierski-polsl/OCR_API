using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class UploadedModelConfiguration : IEntityTypeConfiguration<UploadedModel>
    {
        public void Configure(EntityTypeBuilder<UploadedModel> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.HasIndex(e => e.UserId, "uploaded_model_ibfk_1");

            builder.ToTable("uploaded_models");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Path).HasColumnName("path").IsRequired();
            builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(e => e.UploadTime).HasColumnName("upload_time").IsRequired();

            builder.HasOne(r => r.User)
                .WithMany(u => u.UploadedModels)
                .HasForeignKey(user => user.UserId)
                .HasConstraintName("uploaded_models_ibfk_1");

        }
    }
}
