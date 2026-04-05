using Mediator;

namespace Nkkonsult.Application.Team.Commands;

// Handler dans Infrastructure (RemoveMemberCommandHandler.cs) car accès à ApplicationUser (Identity)
public record RemoveMemberCommand(Guid UserId) : IRequest;
