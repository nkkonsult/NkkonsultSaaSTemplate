namespace Hoplo.Shared.Requests;

public record ResetPasswordRequest(string Email, string Code, string NewPassword);
