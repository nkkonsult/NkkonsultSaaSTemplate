using Microsoft.EntityFrameworkCore;
using Hoplo.Application.Common.Interfaces;
using Hoplo.Domain.Entities;
using Hoplo.Domain.Enums;
using Mediator;

namespace Hoplo.Application.Team.Commands;

public record InviteMemberCommand(string Email) : IRequest<Guid>;

public class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITenantService _tenantService;
    private readonly IAuthService _authService;
    private readonly IUser _user;

    public InviteMemberCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ITenantService tenantService,
        IAuthService authService,
        IUser user)
    {
        _context = context;
        _emailService = emailService;
        _tenantService = tenantService;
        _authService = authService;
        _user = user;
    }

    public async ValueTask<Guid> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        // Déduplication : retour idempotent si invitation en attente existe déjà
        var existing = await _context.Invitations
            .Where(i => i.TenantId == tenantId && i.Email == normalizedEmail && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
            return existing.Id;

        await _authService.CreateInvitedUserAsync(normalizedEmail, tenantId, UserRole.Member, cancellationToken);

        var token = Guid.NewGuid();
        var createdByUserId = Guid.TryParse(_user.Id, out var parsedUserId) ? parsedUserId : Guid.Empty;

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = normalizedEmail,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        await _emailService.SendInvitationEmailAsync(normalizedEmail, token, cancellationToken);

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);

        return invitation.Id;
    }
}
