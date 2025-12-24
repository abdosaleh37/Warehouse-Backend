using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Auth;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;
using WarehouseEntity = Warehouse.Entities.Entities.Warehouse;

namespace Warehouse.DataAccess.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        WarehouseDbContext context,
        IMapper mapper,
        ResponseHandler responseHandler,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Response<RegisterResponse>> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering new user: {UserName}", request.UserName);

        if (request.Password != request.ConfirmPassword)
        {
            return _responseHandler.BadRequest<RegisterResponse>("Password and confirm password do not match.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            _logger.LogWarning("User creation failed for {UserName}: {Errors}", request.UserName, createResult.Errors);
            var errors = string.Join(';', createResult.Errors.Select(e => e.Description));
            return _responseHandler.BadRequest<RegisterResponse>(errors);
        }

        var warehouse = new WarehouseEntity
        {
            Id = Guid.NewGuid(),
            Name = $"{request.UserName}-warehouse",
            UserId = user.Id
        };

        try
        {
            await _context.Warehouses.AddAsync(warehouse, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create warehouse for user {UserName}", request.UserName);
            await _userManager.DeleteAsync(user);
            return _responseHandler.ServerError<RegisterResponse>("Failed to create warehouse for user");
        }

        // Attach warehouse to user in-memory
        user.Warehouse = warehouse;

        var response = new RegisterResponse
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name
        };

        _logger.LogInformation("User {UserName} registered successfully with Warehouse {WarehouseId}", user.UserName, warehouse.Id);
        return _responseHandler.Success(response, "User registered successfully");
    }
}
