using Hoplo.Application.Common.Interfaces;

namespace Hoplo.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
    }

    public async ValueTask<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var code = await _authService.GeneratePasswordResetCodeAsync(request.Email, cancellationToken);

        if (code != null)
            await _emailService.SendPasswordResetCodeEmailAsync(request.Email, code, cancellationToken);

        // Toujours retourner true pour ne pas divulguer si l'email existe
        return true;
    }
}
