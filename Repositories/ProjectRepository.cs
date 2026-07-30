using backend.Data;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.IRepository;
using backend.GenericRepositories;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class ProjectRepository(AppDbContext context) : GenericRepository<Project>(context), IProjectRepository
{
    public async Task<bool> ProjectExistsAsync(string? name) => await DbSet.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

    public async Task<bool> ManagerExistsAsync(Guid managerId) => await context.Users.AnyAsync(x => x.Id == managerId && x.Role != null && x.Role.Name == "Manager").ConfigureAwait(false);

    public async Task AddProjectAsync(Project project, CancellationToken cancellationToken) => await DbSet.AddAsync(project, cancellationToken).ConfigureAwait(false);

    public async Task<Project?> GetProByIdAsync(Guid id) => await DbSet.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);

    public async Task<bool> DuplicateProjectExistsAsync(Guid projectId, string? name) => await DbSet.AnyAsync(x => x.Id != projectId && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<ProjectResponseDto>> GetAllProjectsAsync()
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.ProjectManager)
            .Select(x => new ProjectResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                ProjectManagerId = x.ProjectManagerId,
                ProjectManagerName = x.ProjectManager != null ? x.ProjectManager.Name ?? "" : "",
                TotalUsers = DbSet.Count(x => x.Id == x.Id)
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.ProjectManager)
            .Where(x => x.Id == id)
            .Select(x => new ProjectResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                ProjectManagerId = x.ProjectManagerId,
                ProjectManagerName = x.ProjectManager != null ? x.ProjectManager.Name ?? "" : "",
                TotalUsers = DbSet.Count(x => x.Id == x.Id)
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> ProjectExistsByIdAsync(Guid projectId) => await DbSet.AnyAsync(x => x.Id == projectId).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<ProjectUserResponseDto>> GetProjectEmployeesAsync(Guid projectId)
    {
        return await context.EmployeeProjects.AsNoTracking()
            .Include(x => x.User)
                .ThenInclude(x => x!.Branch)
            .Include(x => x.User)
                .ThenInclude(x => x!.Department)
            .Include(x => x.User)
                .ThenInclude(x => x!.Position)
            .Include(x => x.User)
                .ThenInclude(x => x!.Role)
            .Where(x => x.ProjectId == projectId)
            .Select(x => new ProjectUserResponseDto
            {
                UserId = x.User != null ? x.User.Id : Guid.Empty,
                Name = x.User != null ? x.User.Name ?? "" : "",
                Email = x.User != null ? x.User.Email ?? "" : "",
                DOB = x.User != null ? x.User.DOB : null,
                BranchName = x.User != null && x.User.Branch != null ? x.User.Branch.Name : "",
                DepartmentName = x.User != null && x.User.Department != null ? x.User.Department.Name : "",
                PositionName = x.User != null && x.User.Position != null ? x.User.Position.Name : "",
                RoleName = x.User != null && x.User.Role != null ? x.User.Role.Name : ""
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
