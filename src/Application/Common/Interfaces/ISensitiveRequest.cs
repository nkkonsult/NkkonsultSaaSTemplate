namespace Nkkonsult.Application.Common.Interfaces;

public interface ISensitiveRequest
{
    IReadOnlyList<string> SensitiveProperties { get; }
}
