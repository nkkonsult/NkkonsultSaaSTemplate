using Mediator;

namespace Hoplo.Application.Team.Queries;

public record TeamMemberResult(Guid UserId, string Email, string FirstName, string LastName, string Role, string Status, string? ProfilePhotoUrl);
public record InvitationResult(string Email, DateTime ExpiresAt, string Status);
public record TeamMembersResult(IReadOnlyList<TeamMemberResult> Members, IReadOnlyList<InvitationResult> PendingInvitations);

// Handler dans Infrastructure (GetTeamMembersQueryHandler.cs) car accès à ApplicationUser (Identity)
public record GetTeamMembersQuery : IRequest<TeamMembersResult>;
