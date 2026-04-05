using Hoplo.Application.Common.Interfaces;
using Hoplo.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Hoplo.Web.Controllers.v1;

/// <summary>
/// Endpoints de développement — désactivés en production.
/// </summary>
[ApiController]
[Route("api/v1/dev")]
public class DevController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _env;

    public DevController(
        AppDbContext context,
        ITenantService tenantService,
        ICurrentUserService currentUserService,
        IWebHostEnvironment env)
    {
        _context = context;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
        _env = env;
    }

    /// <summary>
    /// Vérifie que l'environnement de développement est actif.
    /// </summary>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Ping()
    {
        if (!_env.IsDevelopment())
            return Forbid();

        return Ok(new { environment = "Development", timestamp = DateTimeOffset.UtcNow });
    }
}
