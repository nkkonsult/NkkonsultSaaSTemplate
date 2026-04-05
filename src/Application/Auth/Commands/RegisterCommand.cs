using Nkkonsult.Application.Common.Interfaces;

namespace Nkkonsult.Application.Auth.Commands;

// T5.1 — RegisterCommand : crée Tenant + ApplicationUser + période d'essai 60j
public record RegisterCommand(string Email, string Password, string? CompanyName)
    : IRequest<AuthResult?>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["Password"];
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult?>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthResult?> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.Email, request.Password, request.CompanyName);
    }
}
