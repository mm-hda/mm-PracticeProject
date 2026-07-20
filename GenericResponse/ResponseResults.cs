using backend.Dto;
using backend.Dto.CommonDtos;

using System.Text.Json.Serialization;

namespace backend.GenericResponse;

internal sealed class ResponseResults<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetaDto? Meta { get; set; }
    public ResponseStatusCode StatusCode { get; set; } = new();

    public static ResponseResults<T> Success(int StatusCode, T? data = default, PaginationMetaDto? meta = null)
    {
        if (meta == null)
        {
            return new ResponseResults<T>
            {
                Data = data,
                StatusCode = new ResponseStatusCode { StatusCode = StatusCode }
            };
        }
        if (data == null)
        {
            return new ResponseResults<T>
            {
                StatusCode = new ResponseStatusCode { StatusCode = StatusCode }
            };
        }
        return new ResponseResults<T>
        {
            Data = data,
            Meta = meta,
            StatusCode = new ResponseStatusCode { StatusCode = StatusCode }
        };
    }

    public static ResponseResults<T> Failure(int StatusCode)
    {
        return new ResponseResults<T>
        {
            StatusCode = new ResponseStatusCode { StatusCode = StatusCode }
        };
    }

    internal object? Failure(object value, TokenDto item2) => throw new NotImplementedException();
}

internal sealed class ResponseStatusCode
{
    public int StatusCode { get; set; }
}
