namespace Hoplo.Shared.DTOs;

public record UserProfileDto(
    Guid Id, string Email, string FirstName, string LastName,
    string? PhoneNumber, string? ProfilePhotoUrl, string Role
);
