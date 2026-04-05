using Microsoft.AspNetCore.Identity;
using Hoplo.Domain.Entities;
using Hoplo.Domain.Enums;

namespace Hoplo.Infrastructure.Identity;

// Note architecturale : ApplicationUser conservé dans Infrastructure (Clean Architecture).
// La story 1.3 spécifie Domain/Entities mais IdentityUser<Guid> est une dépendance
// infrastructure — ce placement est intentionnel.
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfilePhotoUrl { get; set; }
    public Tenant? Tenant { get; set; }
}
