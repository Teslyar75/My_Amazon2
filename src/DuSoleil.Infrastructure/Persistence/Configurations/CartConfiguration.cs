using DuSoleil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuSoleil.Infrastructure.Persistence.Configurations;

/// <summary>Таблица Carts — корзина гостя (SessionId) или пользователя (UserId).</summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.SessionId).HasMaxLength(100);

        // Индексы для быстрого поиска корзины по пользователю или сессии
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId);
    }
}
