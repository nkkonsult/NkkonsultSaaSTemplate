namespace Nkkonsult.Shared.DTOs;

// T5.4 — DTO renvoyé par GET /api/v1/tenants/me
public record TenantDto(
    Guid Id,
    string Name,
    string Siret,
    string? Siren,
    string? LogoUrl,
    string? Address,
    string? PostalCode,
    string? City,
    string? VatNumber,
    bool OnboardingComplete,
    DateTime TrialEndDate
);
