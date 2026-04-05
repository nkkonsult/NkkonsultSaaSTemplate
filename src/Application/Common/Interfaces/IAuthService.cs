using Nkkonsult.Domain.Enums;

namespace Nkkonsult.Application.Common.Interfaces;

// Résultat d'authentification local à Application (le contrôleur mappe vers AuthResponseDto)
public record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    Guid TenantId,
    string Role
);

/// <summary>
/// Service d'authentification — abstraction Application pour éviter la dépendance
/// directe sur ASP.NET Identity (Infrastructure concern).
/// </summary>
public interface IAuthService
{
    /// <summary>Inscrit un nouvel utilisateur : crée Tenant + ApplicationUser + tokens.</summary>
    Task<AuthResult?> RegisterAsync(string email, string password, string? companyName);

    /// <summary>Connecte un utilisateur et retourne les tokens si les credentials sont valides.</summary>
    Task<AuthResult?> LoginAsync(string email, string password);

    /// <summary>Rotation du refresh token et génération d'un nouveau pair de tokens.</summary>
    Task<AuthResult?> RefreshAsync(string refreshToken);

    /// <summary>Accepte une invitation : crée ou met à jour l'utilisateur, retourne les tokens.</summary>
    Task<AuthResult?> AcceptInvitationAsync(Guid token, string email, string password, CancellationToken cancellationToken = default);

    /// <summary>Crée le compte SystemAdmin protégé par le bootstrap token.</summary>
    Task<AuthResult?> CreateAdminAsync(string email, string password);

    /// <summary>Crée un compte utilisateur pour un invité avec mot de passe aléatoire. Retourne true si créé, false si l'utilisateur existe déjà.</summary>
    Task<bool> CreateInvitedUserAsync(string email, Guid tenantId, UserRole role, CancellationToken cancellationToken = default);

    /// <summary>Génère un code OTP 6 chiffres pour le reset password, le persiste en DB, retourne le code brut (ou null si email inconnu).</summary>
    Task<string?> GeneratePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Vérifie le code OTP de reset et réinitialise le mot de passe si valide.</summary>
    Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>Génère un code OTP 6 chiffres, le persiste en DB, retourne le code brut (ou null si email inconnu).</summary>
    Task<string?> GenerateLoginCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Valide le code OTP et retourne les tokens si valide.</summary>
    Task<AuthResult?> VerifyLoginCodeAsync(string email, string code, CancellationToken cancellationToken = default);
}
