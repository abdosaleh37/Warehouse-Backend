using Warehouse.Entities.DTO.Auth;
using Warehouse.Entities.Shared.ResponseHandling;
using System.Security.Claims;

namespace Warehouse.DataAccess.Services.AuthService;

public interface IAuthService
{
    Task<Response<RegisterResponse>> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default);

    Task<Response<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken);

    Task<Response<object>> LogoutAsync(ClaimsPrincipal user);
}
