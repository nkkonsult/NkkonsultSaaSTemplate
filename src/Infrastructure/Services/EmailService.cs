using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Nkkonsult.Application.Common.Interfaces;

namespace Nkkonsult.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendInvitationEmailAsync(string toEmail, Guid token, CancellationToken cancellationToken = default)
    {
        var html = """
            <p>Bonjour,</p>
            <p>Vous avez été invité à rejoindre une équipe sur <strong>Nkkonsult</strong>, l'application de gestion pour les serruriers.</p>
            <p>Pour vous connecter, ouvrez l'application et utilisez l'une des méthodes suivantes :</p>
            <ul style='padding-left:20px;'>
                <li style='margin-bottom:8px;'><strong>Connexion par code OTP</strong> : entrez votre adresse email et recevez un code de connexion par email.</li>
                <li style='margin-bottom:8px;'><strong>Réinitialisation du mot de passe</strong> : utilisez la fonction « Mot de passe oublié » pour définir votre mot de passe.</li>
            </ul>
            <p>À bientôt sur Nkkonsult !</p>
            <p style='color:#888;font-size:12px;margin-top:24px;'>Si vous n'avez pas demandé cette invitation, vous pouvez ignorer cet email.</p>
            """;

        await SendEmailAsync(toEmail, "Vous êtes invité à rejoindre Nkkonsult", html, cancellationToken);
    }

    public async Task SendPasswordResetCodeEmailAsync(string toEmail, string code, CancellationToken cancellationToken = default)
    {
        var html = $"""
            <p>Votre code de réinitialisation de mot de passe Nkkonsult :</p>
            <p style='font-size:32px;font-weight:bold;letter-spacing:8px;text-align:center;padding:16px;background:#f5f5f5;border-radius:8px;'>{code}</p>
            <p>Ce code expire dans 10 minutes. Si vous n'avez pas fait cette demande, ignorez cet email.</p>
            """;

        await SendEmailAsync(toEmail, "Nkkonsult — Réinitialisation de mot de passe", html, cancellationToken);
    }

    public async Task SendLoginCodeEmailAsync(string toEmail, string code, CancellationToken cancellationToken = default)
    {
        var html = $"""
            <p>Votre code de connexion Nkkonsult :</p>
            <p style='font-size:32px;font-weight:bold;letter-spacing:8px;text-align:center;padding:16px;background:#f5f5f5;border-radius:8px;'>{code}</p>
            <p>Ce code expire dans 10 minutes. Si vous n'avez pas demandé ce code, ignorez cet email.</p>
            """;

        await SendEmailAsync(toEmail, "Nkkonsult — Votre code de connexion", html, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var user = _configuration["SmtpSettings:User"] ?? string.Empty;
        var password = _configuration["SmtpSettings:Password"] ?? string.Empty;
        var from = _configuration["SmtpSettings:From"] ?? "noreply@hoplo.fr";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        if (!string.IsNullOrEmpty(user))
            await client.AuthenticateAsync(user, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
