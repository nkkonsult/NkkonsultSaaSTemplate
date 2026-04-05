namespace Hoplo.Infrastructure.Data;

/// <summary>
/// OTP code for passwordless login or password reset. Stored hashed, with expiration and attempt limit.
/// </summary>
public class LoginCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Purpose { get; set; } = "login";
}
