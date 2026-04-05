namespace Hoplo.Shared.Requests;

// T7.2 — Requête d'inscription
public record RegisterRequest(
    string Email,
    string Password,
    string? CompanyName,
    int[] TradeIds
);
