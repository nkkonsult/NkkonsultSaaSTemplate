namespace Hoplo.Shared.DTOs;

public record TeamMemberDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Status,  // "Active" ou "Inactive"
    string? ProfilePhotoUrl
);
