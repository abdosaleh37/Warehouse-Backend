using Serilog;
using Warehouse.Api.Extensions;
using Warehouse.DataAccess.Extensions;

namespace Warehouse.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("Logs/warehouse-log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            Log.Information("Starting Warehouse API");

            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog();

            // Add DataAccess dependencies (DbContext, Identity, Services, Mapster)
            builder.Services.AddDataAccessDependencies(builder.Configuration);

            // Add API dependencies (Auth, Swagger, FluentValidation, CORS)
            builder.Services.AddApiDependencies(builder.Configuration);

            // Add health checks
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Global exception handler (production)
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/api/error");
                app.UseHsts();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseOpenApi();
                app.UseSwaggerUi(settings =>
                {
                    settings.DocExpansion = "list";
                });
            }

            app.UseHttpsRedirection();

            app.UseCors("WarehousePolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.UseStaticFiles();

            app.MapHealthChecks("/health");

            Log.Information("Warehouse API started successfully");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Warehouse API failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
