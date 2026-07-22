using DuSoleil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuSoleil.Infrastructure.Persistence.Configurations;

public class AdminUserSeedConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Тот же Admin user, что в homework
        builder.HasData(new User
        {
            Id = UserAccessConfiguration.AdminUserId,
            Name = "Administrator",
            Email = "admin@dusoleil.local",
            Avatar = string.Empty,
            RegisteredAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
