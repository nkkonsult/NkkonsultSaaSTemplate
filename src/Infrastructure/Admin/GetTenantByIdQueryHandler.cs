using Hoplo.Application.Admin.Queries;
using Hoplo.Domain.Enums;
using Hoplo.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hoplo.Infrastructure.Admin;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, AdminTenantResult?>
{
    private readonly AppDbContext _context;

    public GetTenantByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async ValueTask<AdminTenantResult?> Handle(
        GetTenantByIdQuery request,
        CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters() justifié : rôle AdminSystème — accès cross-tenant intentionnel
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.Id == request.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
            return null;

        var ownerEmail = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == request.TenantId && u.Role == UserRole.Patron && u.IsActive)
            .Select(u => u.Email ?? string.Empty)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new AdminTenantResult(
            tenant.Id,
            tenant.Name,
            tenant.Siret,
            ownerEmail,
            AdminSubscriptionHelper.GetSubscriptionStatus(tenant.IsActive, tenant.TrialEndDate),
            tenant.TrialEndDate,
            0);  // InterventionCount = 0 jusqu'à la création de l'entité Intervention (Sprint 2)
    }
}
