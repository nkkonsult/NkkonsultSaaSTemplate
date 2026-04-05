namespace Hoplo.Shared.DTOs;

// T7.1 — DTO de réponse auth uniforme (access + refresh token)
public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    Guid TenantId,
    string Role
);
