using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Nkkonsult.Application.Common.Interfaces;
using Nkkonsult.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Nkkonsult.Infrastructure.Services;

// T3.1 — TokenService : génère JWT (15min) + refresh token GUID (7j)
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public TokenService(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateAccessToken(Guid userId, Guid tenantId, string role, string email)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey non configurée");
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId.ToString()),
            new Claim("tenantId", tenantId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateAndPersistRefreshTokenAsync(Guid userId)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashToken(rawToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync(CancellationToken.None);

        return rawToken;
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                !rt.IsUsed &&
                rt.ExpiresAt > DateTime.UtcNow);

        return token?.UserId;
    }

    // Atomic validate-and-rotate : évite les race conditions replay token
    public async Task<string> RotateRefreshTokenAsync(string oldRefreshToken, Guid userId)
    {
        var oldHash = HashToken(oldRefreshToken);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Filtre !IsUsed + ExpiresAt : rejette les tokens déjà consommés ou expirés
        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == oldHash &&
                rt.UserId == userId &&
                !rt.IsUsed &&
                rt.ExpiresAt > DateTime.UtcNow);

        if (existing == null)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Refresh token invalide ou déjà utilisé.");
        }

        existing.IsUsed = true;
        await _context.SaveChangesAsync(CancellationToken.None);

        var newToken = await GenerateAndPersistRefreshTokenAsync(userId);
        await transaction.CommitAsync();

        return newToken;
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}
