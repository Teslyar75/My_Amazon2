using Perry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perry.Infrastructure.Persistence.Configurations;

/// <summary>
/// Seed админа как в homework: Login=Admin, Password=Admin.
/// </summary>
public class UserAccessConfiguration : IEntityTypeConfiguration<UserAccess>
{
    public static readonly Guid AdminUserId = Guid.Parse("53759101-7DE4-4E04-833A-884752290FA0");
    public static readonly Guid AdminAccessId = Guid.Parse("2570A0D2-FAB2-4DE0-8EFC-E2BD28DE2502");
    public const string AdminSalt = "4FA5D20B-E546-4818-9381-B4BD9F327F4E";
    public const string AdminDk = "1678112717E7AF0947F6"; // password = Admin (PbKdf1)

    public void Configure(EntityTypeBuilder<UserAccess> builder)
    {
        builder.ToTable("UserAccesses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoleId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Login).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Salt).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Dk).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Login).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Accesses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new UserAccess
        {
            Id = AdminAccessId,
            UserId = AdminUserId,
            RoleId = "Admin",
            Login = "Admin",
            Salt = AdminSalt,
            Dk = AdminDk
        });
    }
}
