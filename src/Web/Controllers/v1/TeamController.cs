using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkkonsult.Application.Team.Commands;
using Nkkonsult.Application.Team.Queries;
using Nkkonsult.Shared.DTOs;
using Nkkonsult.Shared.Requests;
using Mediator;

namespace Nkkonsult.Web.Controllers.v1;

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
    public async Task<IActionResult> InviteMember([FromBody] InviteMemberRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new InviteMemberCommand(request.Email), ct);
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
    public async Task<IActionResult> RemoveMember(Guid userId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveMemberCommand(userId), ct);
        return NoContent();
    }
}
