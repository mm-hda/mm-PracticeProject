namespace backend.Dto.ProjectDto
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid ProjectManagerId { get; set; }
        public string ProjectManagerName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
    }
}
