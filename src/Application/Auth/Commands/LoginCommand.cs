using Hoplo.Application.Common.Interfaces;

namespace Hoplo.Application.Auth.Commands;

// T5.2 — LoginCommand : valide credentials + génère tokens
public record LoginCommand(string Email, string Password)
    : IRequest<AuthResult?>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["Password"];
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult?>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthResult?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password);
    }
}
