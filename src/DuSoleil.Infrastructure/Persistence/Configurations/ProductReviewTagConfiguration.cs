using DuSoleil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuSoleil.Infrastructure.Persistence.Configurations;

/// <summary>Таблица ProductReviewTags — теги для блока Frequent tags на Product Page.</summary>
public class ProductReviewTagConfiguration : IEntityTypeConfiguration<ProductReviewTag>
{
    public void Configure(EntityTypeBuilder<ProductReviewTag> builder)
    {
        builder.ToTable("ProductReviewTags");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.Review)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
