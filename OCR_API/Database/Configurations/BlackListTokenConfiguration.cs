using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class BlackListTokenConfiguration : IEntityTypeConfiguration<BlackListToken>
    {
        public void Configure(EntityTypeBuilder<BlackListToken> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.UserId, "black_listed_tokens_ibfk_1");

            builder.ToTable("black_listed_tokens");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Token).HasColumnName("token").IsRequired();

            builder.HasOne(d => d.User)
              .WithMany(r => r.BlackListedTokens)
              .HasForeignKey(d => d.UserId)
              .HasConstraintName("black_listed_tokens_ibfk_1");

        }
    }
}
