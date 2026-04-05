using Mediator;

namespace Hoplo.Application.Admin.Queries;

// Handler dans Infrastructure (GetTenantByIdQueryHandler.cs) car accès à AppDbContext.Users (Identity)
public record GetTenantByIdQuery(Guid TenantId) : IRequest<AdminTenantResult?>;
