namespace backend.Dto.BranchDtos;

public class BranchResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
}
