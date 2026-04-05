using Hoplo.Application.Common.Interfaces;

namespace Hoplo.Application.Users.Commands;

public record CompleteOnboardingCommand : IRequest<Unit>;

public class CompleteOnboardingCommandHandler
    : IRequestHandler<CompleteOnboardingCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CompleteOnboardingCommandHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async ValueTask<Unit> Handle(
        CompleteOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tenantOrNull = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        var tenant = Guard.Against.NotFound(tenantId, tenantOrNull);

        tenant.OnboardingComplete = true;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
