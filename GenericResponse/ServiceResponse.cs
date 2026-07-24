using backend.Dto.CommonDtos;

namespace backend.GenericResponse;

public sealed class ServiceResponse<T>
{
    public bool IsSuccess { get; init; }

    public T? Data { get; init; }

    public PaginationMetaDto? Meta { get; init; }

    public int StatusCode { get; init; }
}
