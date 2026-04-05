using Microsoft.EntityFrameworkCore;
using Hoplo.Domain.Entities;

namespace Hoplo.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Invitation> Invitations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
