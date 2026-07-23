using Perry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perry.Infrastructure.Persistence.Configurations;

/// <summary>Таблица ProductReviewImages — фото, приложенные к отзыву.</summary>
public class ProductReviewImageConfiguration : IEntityTypeConfiguration<ProductReviewImage>
{
    public void Configure(EntityTypeBuilder<ProductReviewImage> builder)
    {
        builder.ToTable("ProductReviewImages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();

        builder.HasOne(x => x.Review)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
