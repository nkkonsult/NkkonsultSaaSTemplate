using System.Security.Cryptography;
using System.Text;
using Hoplo.Application.Common.Interfaces;
using Hoplo.Domain.Entities;
using Hoplo.Domain.Enums;
using Hoplo.Infrastructure.Data;
using Hoplo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hoplo.Infrastructure.Services;

// Implémentation IAuthService — gère Register, Login, Refresh avec Identity + TokenService
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    // T5.1 — Inscription : crée Tenant + ApplicationUser avec période d'essai 60j
    // Transaction explicite : évite les Tenants orphelins si CreateAsync échoue
    public async Task<AuthResult?> RegisterAsync(string email, string password, string? companyName)
    {
        // Vérifier email unique
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null) return null; // email déjà pris — même réponse qu'échec (AC #4)

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Créer Tenant avec période d'essai 60 jours (AC #1)
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = companyName ?? string.Empty,
                Siret = string.Empty,
                TrialEndDate = DateTime.UtcNow.AddDays(60),
                IsActive = true,
                OnboardingComplete = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync(CancellationToken.None);

            // Créer ApplicationUser lié au Tenant
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                TenantId = tenant.Id,
                Role = Domain.Enums.UserRole.Owner
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Assigner rôle Owner par défaut
            await _userManager.AddToRoleAsync(user, "Owner");

            await transaction.CommitAsync();
            return await GenerateAuthResultAsync(user, "Owner");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // T5.2 — Connexion : valide credentials via UserManager
    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        // IgnoreQueryFilters() nécessaire : le QueryFilter multi-tenant filtre par TenantId,
        // or lors du login il n'y a pas encore de tenant context (pas de JWT).
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        // AC #4 — même réponse que email inexistant (ne pas divulguer l'existence du compte)
        if (user == null) return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Owner";

        return await GenerateAuthResultAsync(user, role);
    }

    // T5.3 — Refresh : valide token + rotation atomique (AC #6)
    public async Task<AuthResult?> RefreshAsync(string refreshToken)
    {
        var userId = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
        if (userId == null) return null;

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Owner";

        string newRefreshToken;
        try
        {
            // RotateRefreshTokenAsync est atomique : valide + invalide + génère en une transaction
            newRefreshToken = await _tokenService.RotateRefreshTokenAsync(refreshToken, userId.Value);
        }
        catch (InvalidOperationException)
        {
            // Token consommé entre ValidateRefreshTokenAsync et RotateRefreshTokenAsync (race condition)
            return null;
        }

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TenantId, role, user.Email!);

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id,
            TenantId: user.TenantId,
            Role: role
        );
    }

    // T5.4 — Acceptation d'invitation : crée ou met à jour l'utilisateur, retourne les tokens
    public async Task<AuthResult?> AcceptInvitationAsync(Guid token, string email, string password, CancellationToken cancellationToken = default)
    {
        // Pas de QueryFilter sur Invitations (pas de tenant context dans cet appel public)
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

        if (invitation == null
            || invitation.IsUsed
            || invitation.ExpiresAt <= DateTime.UtcNow
            || !invitation.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            return null; // Token invalide, expiré ou email non concordant — réponse générique (pas de fuite d'info)

        // Transaction : CreateUser + marquage IsUsed doivent être atomiques
        // Si SaveChangesAsync échoue après CreateAsync, l'invitation resterait valide → double inscription possible
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Chercher utilisateur existant ou créer
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    TenantId = invitation.TenantId,
                    Role = UserRole.Member,
                    IsActive = true,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                };
                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return null;
                }
            }
            else
            {
                // Utilisateur existant : mettre à jour TenantId et rôle
                user.TenantId = invitation.TenantId;
                user.Role = UserRole.Member;
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
            }

            // Assigner rôle ASP.NET Identity
            if (!await _userManager.IsInRoleAsync(user, "Member"))
                await _userManager.AddToRoleAsync(user, "Member");

            // Marquer invitation utilisée
            invitation.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Générer tokens
            return await GenerateAuthResultAsync(user, "Member");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // Crée un compte invité avec mot de passe aléatoire (l'utilisateur se connectera via OTP ou reset password)
    public async Task<bool> CreateInvitedUserAsync(string email, Guid tenantId, UserRole role, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null) return false; // Utilisateur existe déjà

        // Mot de passe aléatoire non partagé — l'invité se connecte via OTP ou reset password
        var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) + "!1aA";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            TenantId = tenantId,
            Role = role,
            IsActive = true,
            FirstName = string.Empty,
            LastName = string.Empty,
        };

        var result = await _userManager.CreateAsync(user, randomPassword);
        if (!result.Succeeded) return false;

        var roleName = role.ToString();
        if (!await _userManager.IsInRoleAsync(user, roleName))
            await _userManager.AddToRoleAsync(user, roleName);

        return true;
    }

    // Bootstrap — Crée le compte AdminSystème
    // TenantId = Guid.Empty → nécessite un tenant système sentinel en base (FK constraint)
    public async Task<AuthResult?> CreateAdminAsync(string email, string password)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null) return null;

        // Upsert du tenant système sentinel (Id = Guid.Empty) via SQL raw
        // — EF Core remplace Guid.Empty par un Guid auto-généré si on passe par le change tracker
        var trialEnd = DateTime.UtcNow.AddYears(100);
        await _context.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO tenants (id, name, siret, trial_end_date, is_active, onboarding_complete, is_deleted, created_at, updated_at)
            VALUES ({Guid.Empty}, 'Système', '', {trialEnd}, true, true, false, NOW(), NOW())
            ON CONFLICT (id) DO NOTHING
            """);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            TenantId = Guid.Empty,
            Role = UserRole.AdminSysteme,
            IsActive = true,
            FirstName = string.Empty,
            LastName = string.Empty
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded) return null;

        await _userManager.AddToRoleAsync(user, "AdminSystème");
        return await GenerateAuthResultAsync(user, "AdminSystème");
    }

    // Génère un code OTP 6 chiffres pour le reset password
    public async Task<string?> GeneratePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null) return null;

        // Invalider les codes précédents de type password_reset
        var existingCodes = await _context.LoginCodes
            .Where(lc => lc.UserId == user.Id && lc.Purpose == "password_reset" && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingCodes)
            existing.IsUsed = true;

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var loginCode = new LoginCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = HashCode(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Attempts = 0,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            Purpose = "password_reset"
        };

        await _context.LoginCodes.AddAsync(loginCode, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return code;
    }

    // Vérifie le code OTP de reset et réinitialise le mot de passe
    public async Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null) return false;

        var codeHash = HashCode(code);

        var loginCode = await _context.LoginCodes
            .Where(lc => lc.UserId == user.Id && lc.Purpose == "password_reset" && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow && lc.Attempts < 5)
            .OrderByDescending(lc => lc.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (loginCode == null) return false;

        loginCode.Attempts++;

        if (loginCode.CodeHash != codeHash)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        // Code valide — marquer comme utilisé et réinitialiser le mot de passe
        loginCode.IsUsed = true;

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        await _context.SaveChangesAsync(cancellationToken);
        return result.Succeeded;
    }

    // T5.5 — Génère un code OTP 6 chiffres et invalide les codes précédents non utilisés
    public async Task<string?> GenerateLoginCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null) return null;

        // Invalider les codes précédents de type login non utilisés
        var existingCodes = await _context.LoginCodes
            .Where(lc => lc.UserId == user.Id && lc.Purpose == "login" && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingCodes)
            existing.IsUsed = true;

        // Générer un code 6 chiffres cryptographiquement aléatoire
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var loginCode = new LoginCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = HashCode(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Attempts = 0,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            Purpose = "login"
        };

        await _context.LoginCodes.AddAsync(loginCode, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return code;
    }

    // T5.6 — Vérifie le code OTP et retourne un AuthResult si valide
    public async Task<AuthResult?> VerifyLoginCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null) return null;

        var codeHash = HashCode(code);

        // Chercher un code valide de type login (non utilisé, non expiré, < 5 tentatives)
        var loginCode = await _context.LoginCodes
            .Where(lc => lc.UserId == user.Id && lc.Purpose == "login" && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow && lc.Attempts < 5)
            .OrderByDescending(lc => lc.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (loginCode == null) return null;

        loginCode.Attempts++;

        if (loginCode.CodeHash != codeHash)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return null;
        }

        // Code valide — marquer comme utilisé
        loginCode.IsUsed = true;
        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Owner";

        return await GenerateAuthResultAsync(user, role);
    }

    private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user, string role)
    {
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.TenantId, role, user.Email!);
        var refreshToken = await _tokenService.GenerateAndPersistRefreshTokenAsync(user.Id);

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id,
            TenantId: user.TenantId,
            Role: role
        );
    }

    private static string HashCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hash);
    }
}
