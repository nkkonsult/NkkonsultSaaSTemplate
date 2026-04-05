using Hoplo.Application.Common.Interfaces;
using Hoplo.Domain.Entities;
using Hoplo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hoplo.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ITenantService _tenantService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginCode> LoginCodes => Set<LoginCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // QueryFilter soft-delete sur Tenant
        builder.Entity<Tenant>()
            .HasQueryFilter(t => !t.IsDeleted);

        // Index pour la recherche admin par nom
        builder.Entity<Tenant>()
            .HasIndex(t => t.Name)
            .HasDatabaseName("ix_tenants_name");

        // QueryFilter multi-tenant sur ApplicationUser
        builder.Entity<ApplicationUser>()
            .HasQueryFilter(u => u.TenantId == _tenantService.GetCurrentTenantId());

        // ApplicationUser ↔ Tenant
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // RefreshToken
        builder.Entity<RefreshToken>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // LoginCode (OTP magic link)
        builder.Entity<LoginCode>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(lc => lc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // QueryFilter multi-tenant sur Invitation
        builder.Entity<Invitation>()
            .HasQueryFilter(i => i.TenantId == _tenantService.GetCurrentTenantId());
    }
}
