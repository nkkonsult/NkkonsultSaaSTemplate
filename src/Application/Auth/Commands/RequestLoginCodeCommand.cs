using Nkkonsult.Application.Common.Interfaces;

namespace Nkkonsult.Application.Auth.Commands;

public record RequestLoginCodeCommand(string Email) : IRequest<bool>;

public class RequestLoginCodeCommandHandler : IRequestHandler<RequestLoginCodeCommand, bool>
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public RequestLoginCodeCommandHandler(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
    }

    public async ValueTask<bool> Handle(RequestLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var code = await _authService.GenerateLoginCodeAsync(request.Email, cancellationToken);

        if (code != null)
            await _emailService.SendLoginCodeEmailAsync(request.Email, code, cancellationToken);

        // Toujours retourner true pour ne pas divulguer si l'email existe
        return true;
    }
}
