using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hoplo.Application.Team.Commands;
using Hoplo.Application.Team.Queries;
using Hoplo.Shared.DTOs;
using Hoplo.Shared.Requests;
using Mediator;

namespace Hoplo.Web.Controllers.v1;

[ApiController]
[Route("api/v1/team")]
public class TeamController : ControllerBase
{
    private readonly ISender _mediator;

    public TeamController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,SystemAdmin")]
    public async Task<IActionResult> GetTeam(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeamMembersQuery(), ct);
        return Ok(result);
    }

    [HttpPost("invite")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> InviteTechnician([FromBody] InviteTechnicianRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new InviteTechnicianCommand(request.Email), ct);
        return Ok(new { invitationId = id });
    }

    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AcceptInvitationCommand(request.Token, request.Email, request.Password), ct);
        if (result == null)
            return BadRequest(new { message = "Token invalide ou expiré." });
        return Ok(new AuthResponseDto(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.UserId,
            result.TenantId,
            result.Role));
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> RemoveTechnician(Guid userId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveTechnicianCommand(userId), ct);
        return NoContent();
    }
}
