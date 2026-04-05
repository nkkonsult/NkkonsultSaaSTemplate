using Nkkonsult.Application.Common.Interfaces;

namespace Nkkonsult.Application.Auth.Commands;

// T5.3 — RefreshTokenCommand : rotation refresh token + nouveau pair de tokens
public record RefreshTokenCommand(string RefreshToken)
    : IRequest<AuthResult?>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["RefreshToken"];
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult?>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthResult?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshAsync(request.RefreshToken);
    }
}
