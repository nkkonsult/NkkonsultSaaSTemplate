using System.Security.Claims;
using Nkkonsult.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Nkkonsult.Infrastructure.Services;

// T3.2 — TenantService : lit tenantId depuis le claim JWT via IHttpContextAccessor
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentTenantId()
    {
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirstValue("tenantId");

        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Guid.Empty;
        }

        return tenantId;
    }
}
