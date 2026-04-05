namespace Hoplo.Shared.Requests;

// T2.2 — Request body pour PUT /api/v1/tenants/profile
public record UpdateTenantProfileRequest(
    string CompanyName,
    string? Siret,
    string? Siren,
    string? LogoUrl,
    string? Address,
    string? PostalCode,
    string? City,
    string? VatNumber
);
