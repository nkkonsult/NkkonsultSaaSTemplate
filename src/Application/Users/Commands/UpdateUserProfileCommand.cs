using Nkkonsult.Application.Common.Interfaces;

namespace Nkkonsult.Application.Users.Commands;

public record UpdateUserProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Unit>;

public class UpdateUserProfileCommandValidator
    : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100).WithMessage("Le prénom ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Le numéro de téléphone ne doit pas dépasser 20 caractères.")
            .Matches(@"^[\d\s\+\-\.()]*$").WithMessage("Le numéro de téléphone contient des caractères invalides.")
            .When(x => x.PhoneNumber is not null);
    }
}

public class UpdateUserProfileCommandHandler
    : IRequestHandler<UpdateUserProfileCommand, Unit>
{
    private readonly IUserProfileService _userProfileService;
    private readonly IUser _user;

    public UpdateUserProfileCommandHandler(
        IUserProfileService userProfileService,
        IUser user)
    {
        _userProfileService = userProfileService;
        _user = user;
    }

    public async ValueTask<Unit> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = Guard.Against.NullOrEmpty(_user.Id, message: "Utilisateur non authentifié.");
        var parsedId = Guid.Parse(userId);

        var success = await _userProfileService.UpdateProfileAsync(
            parsedId, request.FirstName, request.LastName, request.PhoneNumber, cancellationToken);

        Guard.Against.Expression(x => !x, success, "Utilisateur introuvable.");

        return Unit.Value;
    }
}
