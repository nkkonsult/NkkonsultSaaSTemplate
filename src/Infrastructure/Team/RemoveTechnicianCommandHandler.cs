using Ardalis.GuardClauses;
using Nkkonsult.Application.Common.Interfaces;
using Nkkonsult.Application.Team.Commands;
using Nkkonsult.Domain.Enums;
using Nkkonsult.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Mediator;

namespace Nkkonsult.Infrastructure.Team;

public class RemoveTechnicianCommandHandler : IRequestHandler<RemoveTechnicianCommand>
{
    private readonly AppDbContext _context;
    private readonly IUser _currentUser;

    public RemoveTechnicianCommandHandler(AppDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(RemoveTechnicianCommand request, CancellationToken cancellationToken)
    {
        // M2 — Protection self-delete : un Owner ne peut pas se retirer lui-même
        if (Guid.TryParse(_currentUser.Id, out var currentUserId) && currentUserId == request.UserId)
            throw new InvalidOperationException("Un patron ne peut pas se retirer de sa propre équipe.");

        // QueryFilter ApplicationUser filtre déjà par tenant courant
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        Guard.Against.NotFound(request.UserId, user);

        // M2 — Vérification de rôle : seuls les Members peuvent être retirés
        if (user.Role != UserRole.Member)
            throw new InvalidOperationException("Seuls les techniciens peuvent être retirés de l'équipe.");

        user.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
