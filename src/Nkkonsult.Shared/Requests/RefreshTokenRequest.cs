namespace Hoplo.Shared.Requests;

// T7.4 — Requête de refresh token
public record RefreshTokenRequest(
    string RefreshToken
);
