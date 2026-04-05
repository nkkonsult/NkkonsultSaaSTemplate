namespace Nkkonsult.Shared.Requests;

// T7.3 — Requête de connexion
public record LoginRequest(
    string Email,
    string Password
);
