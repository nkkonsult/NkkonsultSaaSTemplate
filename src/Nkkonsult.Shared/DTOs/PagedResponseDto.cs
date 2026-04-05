namespace Nkkonsult.Shared.DTOs;

public record PagedResponseDto<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
