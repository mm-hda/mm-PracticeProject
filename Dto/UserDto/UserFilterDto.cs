namespace backend.Dto.UserDto
{
    public class UserFilterDto
    {
        public Guid? RoleId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
    }
}