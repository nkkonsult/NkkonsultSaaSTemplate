namespace Nkkonsult.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>Génère un access token JWT (15 min) avec les claims userId, tenantId, role, email.</summary>
    string GenerateAccessToken(Guid userId, Guid tenantId, string role, string email);

    /// <summary>Génère un refresh token GUID aléatoire et le persiste en DB (7 jours).</summary>
    Task<string> GenerateAndPersistRefreshTokenAsync(Guid userId);

    /// <summary>Valide le refresh token et retourne le UserId associé si valide.</summary>
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>Invalide (IsUsed = true) l'ancien token et en génère un nouveau (rotation).</summary>
    Task<string> RotateRefreshTokenAsync(string oldRefreshToken, Guid userId);
}
