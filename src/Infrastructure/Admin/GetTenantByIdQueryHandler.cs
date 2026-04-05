using Nkkonsult.Application.Admin.Queries;
using Nkkonsult.Domain.Enums;
using Nkkonsult.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Nkkonsult.Infrastructure.Admin;

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
        // IgnoreQueryFilters() justifié : rôle SystemAdmin — accès cross-tenant intentionnel
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.Id == request.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
            return null;

        var ownerEmail = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == request.TenantId && u.Role == UserRole.Owner && u.IsActive)
            .Select(u => u.Email ?? string.Empty)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new AdminTenantResult(
            tenant.Id,
            tenant.Name,
            tenant.Siret,
            ownerEmail,
            AdminSubscriptionHelper.GetSubscriptionStatus(tenant.IsActive, tenant.TrialEndDate),
            tenant.TrialEndDate);
    }
}
