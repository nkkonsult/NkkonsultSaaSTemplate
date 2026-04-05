namespace Hoplo.Shared.DTOs;

public record AdminTenantDto(
    Guid Id,
    string Name,
    string Siret,
    string OwnerEmail,
    string SubscriptionStatus,  // "essai" | "actif" | "expiré"
    DateTime TrialEndDate,
    int InterventionCount);
