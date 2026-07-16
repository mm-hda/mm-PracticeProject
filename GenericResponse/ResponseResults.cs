using backend.Dto;
using backend.Dto.Common;
using System.Text.Json.Serialization;


namespace backend.GenericResponse
{
    public class ResponseResults<T>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaginationMetaDto? Meta { get; set; }
        public bool Status { get; set; }
        public string? Message { get; set; }


        public static ResponseResults<T> Success(T? data, string Message)
        {
            return new ResponseResults<T>
            {
                Data = data,
                Message = Message,
                Status = true
            };
        }
        public static ResponseResults<T> Success(T? data, PaginationMetaDto meta, string Message)
        {
            if (meta == null)
            {
                return new ResponseResults<T>
                {
                    Data = data,
                    Message = Message,
                    Status = true
                };
            }
            return new ResponseResults<T>
            {
                Data = data,
                Meta = meta,
                Message = Message,
                Status = true
            };
        }

        public static ResponseResults<T> Failure(T? data, string Message)
        {
            return new ResponseResults<T>
            {
                Message = Message,
                Status = false
            };
        }

        internal static object? Failure(object value, TokenDto item2)
        {
            throw new NotImplementedException();
        }
    }
}