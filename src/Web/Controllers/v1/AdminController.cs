using Nkkonsult.Application.Admin.Queries;
using Nkkonsult.Shared.DTOs;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nkkonsult.Web.Controllers.v1;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "SystemAdmin")]
public class AdminController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminController(ISender mediator)
    {
        _mediator = mediator;
    }

    // GET /api/v1/admin/tenants : liste tous les tenants (pagination + recherche)
    [HttpGet("tenants")]
    [ProducesResponseType(typeof(PagedResponseDto<AdminTenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllTenants(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllTenantsQuery(search, page, pageSize), ct);

        var response = new PagedResponseDto<AdminTenantDto>(
            result.Items.Select(t => new AdminTenantDto(
                t.Id,
                t.Name,
                t.Siret,
                t.OwnerEmail,
                t.SubscriptionStatus,
                t.TrialEndDate)).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);

        return Ok(response);
    }

    // GET /api/v1/admin/tenants/{tenantId} : détail d'un tenant
    [HttpGet("tenants/{tenantId:guid}")]
    [ProducesResponseType(typeof(AdminTenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantDetail(Guid tenantId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery(tenantId), ct);

        if (result is null)
        {
            return Problem(
                detail: $"Tenant {tenantId} introuvable.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5");
        }

        return Ok(new AdminTenantDto(
            result.Id,
            result.Name,
            result.Siret,
            result.OwnerEmail,
            result.SubscriptionStatus,
            result.TrialEndDate));
    }
}
