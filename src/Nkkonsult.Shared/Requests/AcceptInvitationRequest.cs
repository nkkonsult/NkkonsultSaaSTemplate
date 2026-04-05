namespace Hoplo.Shared.Requests;

public record AcceptInvitationRequest(Guid Token, string Email, string Password);
