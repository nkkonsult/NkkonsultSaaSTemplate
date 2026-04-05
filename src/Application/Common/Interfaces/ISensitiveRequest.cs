namespace Hoplo.Application.Common.Interfaces;

public interface ISensitiveRequest
{
    IReadOnlyList<string> SensitiveProperties { get; }
}
