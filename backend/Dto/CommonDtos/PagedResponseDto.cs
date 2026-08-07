using System.Collections.ObjectModel;
namespace backend.Dto.CommonDtos;

public class PagedResponseDto<T>
{
    public ReadOnlyCollection<T> Data { get; } = new([]);
    public PaginationMetaDto Meta { get; set; } = new();
}
