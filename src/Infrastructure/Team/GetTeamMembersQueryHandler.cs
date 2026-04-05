using Ardalis.GuardClauses;
using Hoplo.Application.Common.Interfaces;
using Hoplo.Application.Team.Queries;
using Hoplo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Mediator;

namespace Hoplo.Infrastructure.Team;

public class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, TeamMembersResult>
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;

    public GetTeamMembersQueryHandler(AppDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async ValueTask<TeamMembersResult> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        // QueryFilter ApplicationUser applique déjà TenantId == tenantId
        var members = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => new TeamMemberResult(
                u.Id,
                u.Email ?? string.Empty,
                u.FirstName,
                u.LastName,
                u.Role.ToString(),
                "Active",
                u.ProfilePhotoUrl))
            .ToListAsync(cancellationToken);

        // Invitations sans QueryFilter (filtre manuel)
        var pending = await _context.Invitations
            .Where(i => i.TenantId == tenantId && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
            .Select(i => new InvitationResult(i.Email, i.ExpiresAt, "Pending"))
            .ToListAsync(cancellationToken);

        return new TeamMembersResult(members, pending);
    }
}
