using Perry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perry.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();

        builder.HasData(
            new UserRole
            {
                Id = "Admin",
                Description = "Администратор — полный доступ",
                CanCreate = true,
                CanRead = true,
                CanUpdate = true,
                CanDelete = true
            },
            new UserRole
            {
                Id = "Editor",
                Description = "Редактор каталога",
                CanCreate = true,
                CanRead = true,
                CanUpdate = true,
                CanDelete = false
            },
            new UserRole
            {
                Id = "Guest",
                Description = "Гость / покупатель",
                CanCreate = false,
                CanRead = true,
                CanUpdate = false,
                CanDelete = false
            });
    }
}
