using Microsoft.EntityFrameworkCore;
using Nkkonsult.Domain.Entities;

namespace Nkkonsult.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Invitation> Invitations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
