using Hoplo.Application.Common.Interfaces;
using Mediator;

namespace Hoplo.Application.Auth.Commands;

public record CreateAdminCommand(string Email, string Password)
    : IRequest<AuthResult?>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["Password"];
}

public class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, AuthResult?>
{
    private readonly IAuthService _authService;

    public CreateAdminCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthResult?> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
        => await _authService.CreateAdminAsync(request.Email, request.Password);
}
