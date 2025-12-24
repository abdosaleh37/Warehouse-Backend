using Warehouse.Entities.DTO.Auth;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.AuthService;

public interface IAuthService
{
    Task<Response<RegisterResponse>> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default);

    Task<Response<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
