using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkkonsult.Application.Tenants.Commands;
using Nkkonsult.Application.Tenants.Queries;
using Nkkonsult.Shared.DTOs;
using Nkkonsult.Shared.Requests;

namespace Nkkonsult.Web.Controllers.v1;

[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ISender _mediator;

    public TenantsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // GET /api/v1/tenants/me : profil du tenant courant
    [HttpGet("me")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var result = await _mediator.Send(new GetTenantProfileQuery());

        return Ok(new TenantDto(
            result.Id,
            result.Name,
            result.Siret,
            result.Siren,
            result.LogoUrl,
            result.Address,
            result.PostalCode,
            result.City,
            result.VatNumber,
            result.OnboardingComplete,
            result.TrialEndDate));
    }

    // PUT /api/v1/tenants/profile : mise à jour du profil entreprise
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateTenantProfileRequest request)
    {
        await _mediator.Send(new UpdateTenantProfileCommand(
            request.CompanyName,
            request.Siret,
            request.Siren,
            request.LogoUrl,
            request.Address,
            request.PostalCode,
            request.City,
            request.VatNumber));

        return NoContent();
    }
}
