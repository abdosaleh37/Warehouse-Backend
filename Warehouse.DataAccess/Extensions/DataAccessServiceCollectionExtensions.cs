using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.DataAccess.Services.AuthService;
using Warehouse.DataAccess.Services.CategoryService;
using Warehouse.DataAccess.Services.ExcelExportService;
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

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string for {connectionMode} mode is not configured.");
        }

        services.AddDbContext<WarehouseDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Set command timeout
                sqlOptions.CommandTimeout(30);
            }));

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Password settings - strengthened for production
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
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
        services.AddScoped<IExcelExportService, ExcelExportService>();

        return services;
    }
}
