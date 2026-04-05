using System.Security.Claims;

namespace Nkkonsult.Web.Middleware;

// T4.1 — TenantMiddleware : valide cohérence role/tenant à chaque requête authentifiée
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    // Routes publiques exemptées de la validation tenant
    private static readonly HashSet<string> PublicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/auth/register",
        "/api/v1/auth/login",
        "/api/v1/auth/refresh",
        "/api/v1/auth/setup-admin",
        "/api/v1/auth/forgot-password",
        "/api/v1/auth/reset-password",
        "/api/v1/auth/request-login-code",
        "/api/v1/auth/verify-login-code",
        "/api/v1/team/accept-invitation",
        "/api/v1/trades",
        "/api/v1/companies/search",
        "/health",
        "/scalar",
        "/openapi"
    };

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Routes publiques — passer sans validation
        if (IsPublicRoute(path))
        {
            await _next(context);
            return;
        }

        // Défense en profondeur : bloquer les non-authentifiés sur les routes protégées
        // Même si un contrôleur oublie [Authorize], le middleware bloque ici
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                title = "Unauthorized",
                status = 401,
                detail = "Authentification requise."
            });
            return;
        }

        // Requête authentifiée : vérifier la présence du claim tenantId
        var tenantIdClaim = context.User.FindFirstValue("tenantId");

        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out _))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                title = "Unauthorized",
                status = 401,
                detail = "Token JWT invalide : claim tenantId absent ou invalide."
            });
            return;
        }

        await _next(context);
    }

    private static bool IsPublicRoute(string path)
    {
        foreach (var route in PublicRoutes)
        {
            if (path.StartsWith(route, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
