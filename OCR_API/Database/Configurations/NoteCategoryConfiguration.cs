using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;

namespace OCR_API.Database.Configurations
{
    public class NoteCategoryConfiguration : IEntityTypeConfiguration<NoteCategory>
    {
        public void Configure(EntityTypeBuilder<NoteCategory> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");
            builder.ToTable("note_category_list");
            builder.HasIndex(e => e.UserId, "note_category_list_ibfk_1");

            builder.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired();
            builder.Property(e => e.HexColor).HasColumnName("hex_color").HasDefaultValueSql(null);
            builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();


            builder.HasOne(d => d.User)
                .WithMany(r => r.NoteCategories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("note_category_list_ibfk_1");
            
            builder.HasMany(n => n.Notes)
                .WithMany(c => c.Categories)
                .UsingEntity(j => j.ToTable("note_category_map"));


        }
    }
}
