namespace Nkkonsult.Shared.Requests;

public record VerifyLoginCodeRequest(string Email, string Code);
