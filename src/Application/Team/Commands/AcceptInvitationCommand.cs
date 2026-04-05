using Hoplo.Application.Common.Interfaces;
using Mediator;

namespace Hoplo.Application.Team.Commands;

public record AcceptInvitationCommand(Guid Token, string Email, string Password)
    : IRequest<AuthResult?>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["Token", "Password"];
}

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, AuthResult?>
{
    private readonly IAuthService _authService;

    public AcceptInvitationCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthResult?> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        return await _authService.AcceptInvitationAsync(request.Token, request.Email, request.Password, cancellationToken);
    }
}
