using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Services.TokenService;

public interface ITokenStoreService
{
    Task<string> CreateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken);
    Task InvalidateOldTokensAsync(Guid userId);
    Task<bool> IsValidAsync(string refreshToken);
    Task<(string AccessToken, string RefreshToken)> GenerateAndStoreTokensAsync(Guid userId, ApplicationUser user);
}
