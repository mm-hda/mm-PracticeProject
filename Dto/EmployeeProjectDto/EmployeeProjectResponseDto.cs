namespace backend.Dto.EmployeeProjectDto
{
    public class EmployeeProjectResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
    }
}