namespace Hoplo.Shared.DTOs;

public record InvitationDto(
    string Email,
    DateTime ExpiresAt,
    string Status  // "Pending"
);
