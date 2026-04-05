using Nkkonsult.Application.Common.Interfaces;
using Nkkonsult.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Nkkonsult.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileResult?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        return new UserProfileResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.ProfilePhotoUrl,
            user.Role.ToString());
    }

    public async Task<bool> UpdateProfileAsync(
        Guid userId, string firstName, string lastName, string? phoneNumber, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = phoneNumber;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateProfilePhotoAsync(Guid userId, string photoUrl, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.ProfilePhotoUrl = photoUrl;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
