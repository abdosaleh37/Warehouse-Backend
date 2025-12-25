using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
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
            options.AddPolicy("WarehousePolicy", builder =>
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
             );
        });

        services.AddAuthConfig(configuration)
            .AddSwaggerServices()
            .AddFluentValidationConfig();

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
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwtSettings?.Issuer),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(jwtSettings?.Audience),
                    ValidAudience = jwtSettings?.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SigningKey ?? "")),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Warehouse API",
                Version = "v1",
                Description = "Warehouse Management System API"
            });

            // Add JWT Bearer Authorization
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "please enter valid token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme,
            });

            //options.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Id = JwtBearerDefaults.AuthenticationScheme,
            //                Type = ReferenceType.SecurityScheme,
            //            },

            //        },
            //        Array.Empty<string>()
            //    }
            //});

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
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
}
