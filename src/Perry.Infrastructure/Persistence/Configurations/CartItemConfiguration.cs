using Perry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perry.Infrastructure.Persistence.Configurations;

/// <summary>Таблица CartItems — уникальная пара (CartId, ProductId).</summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(x => x.Id);

        // Один и тот же товар не дублируется в корзине — меняем Quantity
        builder.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();

        builder.HasOne(x => x.Cart)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: товар нельзя удалить, пока он есть в чьей-то корзине
        // (или сначала архивируй товар / чисти корзины)
        builder.HasOne(x => x.Product)
            .WithMany(x => x.CartItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
