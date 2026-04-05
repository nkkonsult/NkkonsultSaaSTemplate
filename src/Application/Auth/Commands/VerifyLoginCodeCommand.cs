using Hoplo.Application.Common.Interfaces;

namespace Hoplo.Application.Auth.Commands;

public record VerifyLoginCodeCommand(string Email, string Code)
    : IRequest<AuthResult?>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["Code"];
}

public class VerifyLoginCodeCommandHandler : IRequestHandler<VerifyLoginCodeCommand, AuthResult?>
{
    private readonly IAuthService _authService;

    public VerifyLoginCodeCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthResult?> Handle(VerifyLoginCodeCommand request, CancellationToken cancellationToken)
    {
        return await _authService.VerifyLoginCodeAsync(request.Email, request.Code, cancellationToken);
    }
}
