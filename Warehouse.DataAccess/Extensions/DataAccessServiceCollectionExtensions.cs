using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.DataAccess.Mappings;
using Warehouse.DataAccess.Services.AuthService;
using Warehouse.DataAccess.Services.CategoryService;
using Warehouse.DataAccess.Services.ItemService;
using Warehouse.DataAccess.Services.ItemVoucherService;
using Warehouse.DataAccess.Services.SectionService;
using Warehouse.DataAccess.Services.TokenService;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Extensions;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddIdentityServices()
            .AddMapsterConfig()
            .AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionMode = configuration.GetValue<string>("ConnectionMode");
        var connectionString = connectionMode == "Prod"
            ? configuration.GetConnectionString("ProdCS")
            : configuration.GetConnectionString("DevCS");

        services.AddDbContext<WarehouseDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<WarehouseDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(typeof(DataAccessServiceCollectionExtensions).Assembly);
        services.AddSingleton<IMapper>(new Mapper(mappingConfig));
        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ResponseHandler>();
        
        // Authentication
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenStoreService, TokenStoreService>();
        
        // Warehouse services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IItemVoucherService, ItemVoucherService>();
        
        return services;
    }
}
