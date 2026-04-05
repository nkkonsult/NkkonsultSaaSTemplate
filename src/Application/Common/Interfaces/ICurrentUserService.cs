namespace Nkkonsult.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
    bool IsInRole(string role);
}
