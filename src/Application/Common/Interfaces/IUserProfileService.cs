namespace Hoplo.Application.Common.Interfaces;

public record UserProfileResult(
    Guid Id, string Email, string FirstName, string LastName,
    string? PhoneNumber, string? ProfilePhotoUrl, string Role
);

/// <summary>
/// Service de gestion du profil utilisateur — abstraction Application
/// pour éviter la dépendance directe sur ASP.NET Identity.
/// </summary>
public interface IUserProfileService
{
    /// <summary>Récupère le profil de l'utilisateur par son ID.</summary>
    Task<UserProfileResult?> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Met à jour le profil utilisateur (prénom, nom, téléphone).</summary>
    Task<bool> UpdateProfileAsync(Guid userId, string firstName, string lastName, string? phoneNumber, CancellationToken ct = default);

    /// <summary>Met à jour l'URL de la photo de profil.</summary>
    Task<bool> UpdateProfilePhotoAsync(Guid userId, string photoUrl, CancellationToken ct = default);
}
