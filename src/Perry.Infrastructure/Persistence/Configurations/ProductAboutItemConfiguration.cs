using Perry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perry.Infrastructure.Persistence.Configurations;

/// <summary>Таблица ProductAboutItems — пункты аккордеона «About product».</summary>
public class ProductAboutItemConfiguration : IEntityTypeConfiguration<ProductAboutItem>
{
    public void Configure(EntityTypeBuilder<ProductAboutItem> builder)
    {
        builder.ToTable("ProductAboutItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.AboutItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
