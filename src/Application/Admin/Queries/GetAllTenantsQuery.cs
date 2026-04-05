using Mediator;

namespace Nkkonsult.Application.Admin.Queries;

// Résultats internes — mappés en DTOs dans le contrôleur
public record AdminTenantResult(
    Guid Id,
    string Name,
    string Siret,
    string OwnerEmail,
    string SubscriptionStatus,
    DateTime TrialEndDate,
    int InterventionCount);

public record GetAllTenantsQueryResult(
    IReadOnlyCollection<AdminTenantResult> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

// Handler dans Infrastructure (GetAllTenantsQueryHandler.cs) car accès à AppDbContext.Users (Identity)
public record GetAllTenantsQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<GetAllTenantsQueryResult>;
