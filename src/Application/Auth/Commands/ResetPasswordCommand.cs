using Hoplo.Application.Common.Interfaces;

namespace Hoplo.Application.Auth.Commands;

public record ResetPasswordCommand(string Email, string Code, string NewPassword)
    : IRequest<bool>, ISensitiveRequest
{
    public IReadOnlyList<string> SensitiveProperties => ["Code", "NewPassword"];
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ResetPasswordWithCodeAsync(request.Email, request.Code, request.NewPassword, cancellationToken);
    }
}
