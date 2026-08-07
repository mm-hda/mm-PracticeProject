namespace backend.Dto.BranchDtos;

public class BranchResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public int TotalUsers { get; set; }
}
