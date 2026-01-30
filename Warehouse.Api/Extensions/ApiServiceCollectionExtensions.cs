using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Warehouse.Entities.Utilities.Configurations;

namespace Warehouse.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiDependencies(this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "*" };

            options.AddPolicy("WarehousePolicy", builder =>
            {
                if (allowedOrigins.Length == 1 && allowedOrigins[0] == "*")
                {
                    // Development: Allow any origin
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
                else
                {
                    // Production: Restrict to specific origins
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                }
            });
        });

        services.AddAuthConfig(configuration)
            .AddSwaggerServices()
            .AddFluentValidationConfig()
            .AddRequestSizeLimits();

        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        return services;
    }

    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JWT").Get<JwtSettings>();

                if (string.IsNullOrEmpty(jwtSettings?.SigningKey))
                {
                    throw new InvalidOperationException("JWT SigningKey is not configured. Set it via user secrets or environment variables.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwtSettings?.Issuer),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(jwtSettings?.Audience),
                    ValidAudience = jwtSettings?.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "v1";
            config.Title = "Warehouse API";
            config.Version = "v1";
            config.Description = "Warehouse Management System API";

            config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter the JWT token"
            });

            config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                config.PostProcess = document => { document.Info = document.Info ?? new OpenApiInfo(); };
            }
        });

        return services;
    }

    private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {
        services
           .AddFluentValidationAutoValidation()
           .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    private static IServiceCollection AddRequestSizeLimits(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
            options.ValueLengthLimit = 10 * 1024 * 1024;
            options.MultipartHeadersLengthLimit = 10 * 1024 * 1024;
        });

        return services;
    }
}