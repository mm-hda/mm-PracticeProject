using backend.Dto;
using backend.Dto.CommonDtos;

using System.Text.Json.Serialization;

namespace backend.GenericResponse;

internal sealed class ResponseResults<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; } = default;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetaDto? Meta { get; set; }
    public int StatusCode { get; set; }

    public static ResponseResults<T> Success(int StatusCode, T? data = default, PaginationMetaDto? meta = null)
    {
        if (data == null && meta == null)
        {
            return new ResponseResults<T>
            {
                StatusCode = StatusCode
            };
        }
        else if (meta == null)
        {
            return new ResponseResults<T>
            {
                Data = data,
                StatusCode = StatusCode
            };
        }
        return new ResponseResults<T>
        {
            Data = data,
            Meta = meta,
            StatusCode = StatusCode
        };
    }

    public static ResponseResults<T> Failure(int StatusCode)
    {
        return new ResponseResults<T>
        {
            StatusCode = StatusCode
        };
    }

    internal object? Failure(object value, TokenDto item2) => throw new NotImplementedException();
}
