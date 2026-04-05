using Hoplo.Application.Common.Interfaces;
using Hoplo.Domain.Entities;

namespace Hoplo.Application.Tenants.Commands;

// T1.1 — Command + Handler : met à jour Name, Siret, LogoUrl du tenant courant
// T1.3 — TenantId provient de ITenantService (jamais du body)
public record UpdateTenantProfileCommand(
    string CompanyName,
    string? Siret,
    string? Siren,
    string? LogoUrl,
    string? Address,
    string? PostalCode,
    string? City,
    string? VatNumber
) : IRequest<Unit>;

public class UpdateTenantProfileCommandHandler
    : IRequestHandler<UpdateTenantProfileCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public UpdateTenantProfileCommandHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async ValueTask<Unit> Handle(
        UpdateTenantProfileCommand request,
        CancellationToken cancellationToken)
    {
        // T1.3 — TenantId issu du claim JWT uniquement
        var tenantId = _tenantService.GetCurrentTenantId();

        var tenantOrNull = await _context.Tenants
            .IgnoreQueryFilters()   // Tenants ne sont pas filtrés par tenant (ils SONT les tenants)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        // Ardalis.GuardClauses.NotFoundException si tenant inexistant
        var tenant = Guard.Against.NotFound(tenantId, tenantOrNull);

        tenant.Name = request.CompanyName;
        tenant.Siret = request.Siret ?? string.Empty;
        tenant.Siren = request.Siren;
        tenant.Address = request.Address;
        tenant.PostalCode = request.PostalCode;
        tenant.City = request.City;
        tenant.VatNumber = request.VatNumber;
        tenant.UpdatedAt = DateTime.UtcNow;

        if (request.LogoUrl is not null)
            tenant.LogoUrl = request.LogoUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

// T1.2 — Validateur FluentValidation : SIRET exactement 14 chiffres
public class UpdateTenantProfileCommandValidator
    : AbstractValidator<UpdateTenantProfileCommand>
{
    public UpdateTenantProfileCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Le nom de l'entreprise est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom ne doit pas dépasser 200 caractères.");

        RuleFor(x => x.Siret)
            .Length(14).WithMessage("Le SIRET doit contenir exactement 14 caractères.")
            .Matches(@"^\d{14}$").WithMessage("Le SIRET doit contenir exactement 14 chiffres.")
            .When(x => !string.IsNullOrEmpty(x.Siret));

        RuleFor(x => x.Siren)
            .Length(9).WithMessage("Le SIREN doit contenir exactement 9 caractères.")
            .Matches(@"^\d{9}$").WithMessage("Le SIREN doit contenir exactement 9 chiffres.")
            .When(x => x.Siren is not null);

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("L'adresse ne doit pas dépasser 500 caractères.")
            .When(x => x.Address is not null);

        RuleFor(x => x.PostalCode)
            .MaximumLength(10).WithMessage("Le code postal ne doit pas dépasser 10 caractères.")
            .When(x => x.PostalCode is not null);

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("La ville ne doit pas dépasser 100 caractères.")
            .When(x => x.City is not null);
    }
}
