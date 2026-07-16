namespace backend.Dto.Common
{
    public class PagedResponseDto<T>
    {
        public List<T> Data { get; set; } = [];
        public PaginationMetaDto Meta { get; set; } = new();
    }
}