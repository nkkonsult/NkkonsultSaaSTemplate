using Microsoft.AspNetCore.Mvc;
using Hoplo.Application.Auth.Commands;
using Hoplo.Shared.DTOs;
using Hoplo.Shared.Requests;

namespace Hoplo.Web.Controllers.v1;

// T6.1 — AuthController : endpoints auth PUBLICS (sans [Authorize])
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(ISender mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    // AC #1 — Inscription
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(
            request.Email,
            request.Password,
            request.CompanyName));

        if (result is null)
        {
            // T6.2 — Réponse 401 RFC 9457 (Content-Type: application/problem+json) — AC #4
            return Problem(
                detail: "Email ou mot de passe incorrect.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
        }

        return Ok(new AuthResponseDto(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.UserId,
            result.TenantId,
            result.Role));
    }

    // AC #2 — Connexion
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password));

        if (result is null)
        {
            // T6.2 — AC #4 : même message que email inexistant (RFC 9457)
            return Problem(
                detail: "Email ou mot de passe incorrect.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
        }

        return Ok(new AuthResponseDto(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.UserId,
            result.TenantId,
            result.Role));
    }

    // Bootstrap admin — protégé par token secret en config (AdminBootstrap:Token)
    // 404 si clé absente/vide (désactivé), 401 si token invalide, 409 si email déjà pris
    [HttpPost("setup-admin")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetupAdmin(
        [FromQuery] string token,
        [FromBody] SetupAdminRequest request)
    {
        var configToken = _configuration["AdminBootstrap:Token"];
        if (string.IsNullOrEmpty(configToken))
            return NotFound();

        if (!string.Equals(token, configToken, StringComparison.Ordinal))
            return Problem(
                detail: "Token invalide.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");

        var result = await _mediator.Send(new CreateAdminCommand(request.Email, request.Password));
        if (result is null)
            return Problem(
                detail: "Compte admin déjà existant ou erreur de création.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.10");

        return Ok(new AuthResponseDto(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.UserId,
            result.TenantId,
            result.Role));
    }

    // AC #5, #6 — Refresh token avec rotation
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));

        if (result is null)
        {
            return Problem(
                detail: "Refresh token invalide ou expiré.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
        }

        return Ok(new AuthResponseDto(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.UserId,
            result.TenantId,
            result.Role));
    }

    // Mot de passe oublié — envoie un code OTP par email
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email));

        // Toujours 200 pour ne pas divulguer si l'email existe
        return Ok(new { message = "Si un compte existe avec cet email, un code de réinitialisation a été envoyé." });
    }

    // Réinitialisation du mot de passe avec code OTP
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _mediator.Send(new ResetPasswordCommand(request.Email, request.Code, request.NewPassword));

        if (!success)
            return Problem(
                detail: "Code invalide ou expiré, ou mot de passe non conforme.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");

        return Ok(new { message = "Mot de passe réinitialisé avec succès." });
    }

    // Demande de code OTP par email
    [HttpPost("request-login-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestLoginCode([FromBody] RequestLoginCodeRequest request)
    {
        await _mediator.Send(new RequestLoginCodeCommand(request.Email));

        // Toujours 200 pour ne pas divulguer si l'email existe
        return Ok(new { message = "Si un compte existe avec cet email, un code de connexion a été envoyé." });
    }

    // Vérification du code OTP — retourne les tokens si valide
    [HttpPost("verify-login-code")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyLoginCode([FromBody] VerifyLoginCodeRequest request)
    {
        var result = await _mediator.Send(new VerifyLoginCodeCommand(request.Email, request.Code));

        if (result is null)
            return Problem(
                detail: "Code invalide ou expiré.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");

        return Ok(new AuthResponseDto(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.UserId,
            result.TenantId,
            result.Role));
    }
}
