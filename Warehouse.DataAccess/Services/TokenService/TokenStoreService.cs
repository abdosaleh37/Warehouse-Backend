using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Services.TokenService;

public class TokenStoreService : ITokenStoreService
{
    private readonly WarehouseDbContext _context;
    private readonly IConfiguration _configuration;

    public TokenStoreService(
        WarehouseDbContext context, 
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> CreateAccessTokenAsync(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JWT").Get<Entities.Utilities.Configurations.JwtSettings>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SigningKey ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty)
        };

        if (user.Warehouse != null)
        {
            claims.Add(new Claim("WarehouseId", user.Warehouse.Id.ToString()));
        }

        var expiry = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: jwtSettings?.Issuer,
            audience: jwtSettings?.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public async Task SaveRefreshTokenAsync(
        Guid userId, 
        string refreshToken)
    {
        var token = new UserRefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiryDateUtc = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.UserRefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsValidAsync(string refreshToken)
    {
        var token = await _context.UserRefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token == null) return false;
        if (token.IsUsed) return false;
        if (token.ExpiryDateUtc <= DateTime.UtcNow) return false;
        return true;
    }

    public async Task InvalidateOldTokensAsync(Guid userId)
    {
        var tokens = await _context.UserRefreshTokens.Where(t => t.UserId == userId && !t.IsUsed).ToListAsync();
        foreach (var t in tokens)
        {
            t.IsUsed = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateAndStoreTokensAsync(
        Guid userId, 
        ApplicationUser user)
    {
        var access = await CreateAccessTokenAsync(user);
        var refresh = GenerateRefreshToken();

        await InvalidateOldTokensAsync(userId);
        await SaveRefreshTokenAsync(userId, refresh);

        return (access, refresh);
    }
}
