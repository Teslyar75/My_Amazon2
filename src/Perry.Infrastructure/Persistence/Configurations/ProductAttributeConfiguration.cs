using Perry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perry.Infrastructure.Persistence.Configurations;

/// <summary>Таблица ProductAttributes — характеристики и фильтры.</summary>
public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();

        // Индекс для быстрого фильтра вида Name=Color, Value=Black
        builder.HasIndex(x => new { x.Name, x.Value });

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Attributes)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
