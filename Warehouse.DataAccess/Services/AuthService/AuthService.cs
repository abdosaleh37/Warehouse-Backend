using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.DataAccess.Services.TokenService;
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
    private readonly ITokenStoreService _tokenStoreService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        WarehouseDbContext context,
        IMapper mapper,
        ResponseHandler responseHandler,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        ITokenStoreService tokenStoreService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
        _configuration = configuration;
        _logger = logger;
        _tokenStoreService = tokenStoreService;
    }

    public async Task<Response<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering new user: {UserName}", request.UserName);

        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                return _responseHandler.BadRequest<RegisterResponse>("Password and confirm password do not match.");
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
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

            await _context.Warehouses.AddAsync(warehouse, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RegisterAsync cancelled for User: {UserName}", request.UserName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering user: {UserName}", request.UserName);
            return _responseHandler.InternalServerError<RegisterResponse>("An error occurred while registering the user.");
        }
    }

    public async Task<Response<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for user: {UserName}", request.UserName);

        try
        {
            var user = await _userManager.Users
                .Include(u => u.Warehouse)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found {UserName}", request.UserName);
                return _responseHandler.NotFound<LoginResponse>("Invalid username or password");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Login failed: invalid password for user {UserName}", request.UserName);
                return _responseHandler.NotFound<LoginResponse>("Invalid username or password");
            }

            // Use TokenStoreService to generate and persist tokens
            var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user.Id, user);

            var response = new LoginResponse
            {
                Token = tokens.AccessToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                RefreshToken = tokens.RefreshToken
            };

            _logger.LogInformation("User {UserName} logged in successfully.", request.UserName);
            return _responseHandler.Success(response, "Login successful");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("LoginAsync cancelled for User: {UserName}", request.UserName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for user: {UserName}", request.UserName);
            return _responseHandler.InternalServerError<LoginResponse>("An error occurred while logging in the user.");
        }
    }

    public async Task<Response<RefreshTokenResponse>> RefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing token");

        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return _responseHandler.Unauthorized<RefreshTokenResponse>("Invalid refresh token");
            }

            // Atomically mark token as used using a single update statement to avoid race conditions
            using var transaction = await _context.Database.BeginTransactionAsync();

            var rows = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE [UserRefreshTokens] SET IsUsed = 1 WHERE Token = {0} AND IsUsed = 0 AND ExpiryDateUtc > {1}",
                refreshToken, DateTime.UtcNow);

            if (rows == 0)
            {
                await transaction.RollbackAsync();
                return _responseHandler.Unauthorized<RefreshTokenResponse>("Invalid refresh token");
            }

            // Now retrieve the token record to get the UserId
            var tokenRecord = await _context.UserRefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

            if (tokenRecord == null)
            {
                await transaction.RollbackAsync();
                return _responseHandler.Unauthorized<RefreshTokenResponse>("Invalid refresh token");
            }

            await transaction.CommitAsync();

            var user = await _userManager.Users.
                Include(u => u.Warehouse).
                FirstOrDefaultAsync(u => u.Id == tokenRecord.UserId, cancellationToken);

            if (user == null)
            {
                return _responseHandler.Unauthorized<RefreshTokenResponse>("Invalid refresh token");
            }

            var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(tokenRecord.UserId, user);

            var response = new RefreshTokenResponse
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };

            return _responseHandler.Success(response, "Token refreshed");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RefreshTokenAsync cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            return _responseHandler.InternalServerError<RefreshTokenResponse>("Failed to process refresh token");
        }
    }

    public async Task<Response<object>> LogoutAsync(ClaimsPrincipal userClaims)
    {
        _logger.LogInformation("Processing logout request");

        try
        {
            if (userClaims == null)
            {
                _logger.LogWarning("Logout failed: ClaimsPrincipal is null");
                return _responseHandler.Unauthorized<object>("Invalid token");
            }

            var sub = userClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
            {
                _logger.LogWarning("Logout failed: unable to determine user id from ClaimsPrincipal");
                return _responseHandler.Unauthorized<object>("Invalid token");
            }

            await _tokenStoreService.InvalidateOldTokensAsync(userId);
            _logger.LogInformation("Refresh tokens invalidated for user: {UserId}", userId);
            return _responseHandler.Success<object>(null!, "Logged out successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("LogoutAsync cancelled for ClaimsPrincipal");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to logout user derived from ClaimsPrincipal");
            return _responseHandler.InternalServerError<object>("Failed to logout user");
        }
    }
}