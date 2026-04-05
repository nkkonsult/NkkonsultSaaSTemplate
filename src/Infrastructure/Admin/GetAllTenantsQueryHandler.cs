using Hoplo.Application.Admin.Queries;
using Hoplo.Domain.Enums;
using Hoplo.Infrastructure.Data;
using Hoplo.Infrastructure.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hoplo.Infrastructure.Admin;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, GetAllTenantsQueryResult>
{
    private readonly AppDbContext _context;

    public GetAllTenantsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async ValueTask<GetAllTenantsQueryResult> Handle(
        GetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters() justifié : rôle AdminSystème — accès cross-tenant intentionnel
        var tenantsQuery = _context.Tenants
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted);  // soft-delete manuel car QueryFilter ignoré

        // Jointure avec les utilisateurs pour obtenir l'email du patron
        var joinedQuery =
            from tenant in tenantsQuery
            join owner in _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.Role == UserRole.Patron && u.IsActive)
                on tenant.Id equals owner.TenantId into owners
            from owner in owners.DefaultIfEmpty()
            select new { Tenant = tenant, OwnerEmail = owner != null ? owner.Email ?? string.Empty : string.Empty };

        // Filtre de recherche par nom d'entreprise ou email du patron (NFR1 — index GIN trgm)
        // ILike génère un ILIKE PostgreSQL natif exploitable par l'index ix_tenants_name_trgm
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search}%";
            joinedQuery = joinedQuery.Where(x =>
                EF.Functions.ILike(x.Tenant.Name, pattern) ||
                EF.Functions.ILike(x.OwnerEmail, pattern));
        }

        var totalCount = await joinedQuery.CountAsync(cancellationToken);

        var items = await joinedQuery
            .OrderBy(x => x.Tenant.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AdminTenantResult(
                x.Tenant.Id,
                x.Tenant.Name,
                x.Tenant.Siret,
                x.OwnerEmail,
                AdminSubscriptionHelper.GetSubscriptionStatus(x.Tenant.IsActive, x.Tenant.TrialEndDate),
                x.Tenant.TrialEndDate,
                0))  // InterventionCount = 0 jusqu'à la création de l'entité Intervention (Sprint 2)
            .ToListAsync(cancellationToken);

        return new GetAllTenantsQueryResult(items, totalCount, request.Page, request.PageSize);
    }

}
