using Hoplo.Application.Common.Interfaces;

namespace Hoplo.Application.Users.Queries;

public record GetUserProfileQuery : IRequest<UserProfileResult>;

public class GetUserProfileQueryHandler
    : IRequestHandler<GetUserProfileQuery, UserProfileResult>
{
    private readonly IUserProfileService _userProfileService;
    private readonly IUser _user;

    public GetUserProfileQueryHandler(
        IUserProfileService userProfileService,
        IUser user)
    {
        _userProfileService = userProfileService;
        _user = user;
    }

    public async ValueTask<UserProfileResult> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = Guard.Against.NullOrEmpty(_user.Id, message: "Utilisateur non authentifié.");
        var parsedId = Guid.Parse(userId);

        var profile = await _userProfileService.GetProfileAsync(parsedId, cancellationToken);

        return Guard.Against.NotFound(parsedId, profile);
    }
}
