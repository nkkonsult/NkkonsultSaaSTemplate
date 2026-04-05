using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkkonsult.Application.Users.Commands;
using Nkkonsult.Application.Users.Queries;
using Nkkonsult.Shared.DTOs;
using Nkkonsult.Shared.Requests;

namespace Nkkonsult.Web.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    // GET /api/v1/users/me — profil de l'utilisateur courant
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var result = await _mediator.Send(new GetUserProfileQuery());

        return Ok(new UserProfileDto(
            result.Id,
            result.Email,
            result.FirstName,
            result.LastName,
            result.PhoneNumber,
            result.ProfilePhotoUrl,
            result.Role));
    }

    // PUT /api/v1/users/profile — mise à jour du profil utilisateur
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        await _mediator.Send(new UpdateUserProfileCommand(
            request.FirstName,
            request.LastName,
            request.PhoneNumber));

        return NoContent();
    }

    // POST /api/v1/users/complete-onboarding — marquer l'onboarding comme terminé
    [HttpPost("complete-onboarding")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOnboarding()
    {
        await _mediator.Send(new CompleteOnboardingCommand());
        return NoContent();
    }
}
