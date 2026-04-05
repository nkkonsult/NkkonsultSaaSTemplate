namespace Hoplo.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Siret { get; set; } = string.Empty;
    public string? Siren { get; set; }
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? VatNumber { get; set; }
    public DateTime TrialEndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool OnboardingComplete { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
