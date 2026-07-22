using DuSoleil.Infrastructure.Auth;
using DuSoleil.Infrastructure.Persistence;
using DuSoleil.Infrastructure.Services;
using DuSoleil.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DuSoleil.Infrastructure;

/// <summary>
/// Регистрация инфраструктурных сервисов в DI-контейнере ASP.NET.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IKdfService, PbKdf1Service>();
        services.AddSingleton<IStorageService, DiskStorageService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IViewedProductsService, ViewedProductsService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
