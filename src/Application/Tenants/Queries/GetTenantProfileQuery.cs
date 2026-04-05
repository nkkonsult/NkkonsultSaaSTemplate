using Hoplo.Application.Common.Interfaces;
using Hoplo.Domain.Entities;

namespace Hoplo.Application.Tenants.Queries;

// T5.4 — Retourner le profil du tenant courant (utilisé par GET /api/v1/tenants/me)
public record GetTenantProfileQuery : IRequest<TenantProfileDto>;

public record TenantProfileDto(
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

public class GetTenantProfileQueryHandler
    : IRequestHandler<GetTenantProfileQuery, TenantProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetTenantProfileQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async ValueTask<TenantProfileDto> Handle(
        GetTenantProfileQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tenantOrNull = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        // Ardalis.GuardClauses.NotFoundException si tenant inexistant
        var tenant = Guard.Against.NotFound(tenantId, tenantOrNull);

        return new TenantProfileDto(
            tenant.Id,
            tenant.Name,
            tenant.Siret,
            tenant.Siren,
            tenant.LogoUrl,
            tenant.Address,
            tenant.PostalCode,
            tenant.City,
            tenant.VatNumber,
            tenant.OnboardingComplete,
            tenant.TrialEndDate);
    }
}
