namespace Nkkonsult.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, Guid token, CancellationToken cancellationToken = default);
    Task SendPasswordResetCodeEmailAsync(string toEmail, string code, CancellationToken cancellationToken = default);
    Task SendLoginCodeEmailAsync(string toEmail, string code, CancellationToken cancellationToken = default);
}
