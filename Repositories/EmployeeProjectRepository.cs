using backend.Data;
using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class EmployeeProjectRepository(AppDbContext context) : IEmployeeProjectRepository
{
    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken) => await context.Users.AnyAsync(x => x.Id == userId, cancellationToken).ConfigureAwait(false);

    public async Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken) => await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken).ConfigureAwait(false);

    public async Task<bool> EmployeeProjectExistsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken) => await context.EmployeeProjects.AnyAsync(x => x.UserId == userId && x.ProjectId == projectId, cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(EmployeeProject employeeProject, CancellationToken cancellationToken) => await context.EmployeeProjects.AddAsync(employeeProject, cancellationToken).ConfigureAwait(false);

    public async Task<EmployeeProject?> GetEmployeeProjectByIdAsync(Guid id, CancellationToken cancellationToken) => await context.EmployeeProjects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public void Remove(EmployeeProject employeeProject) => context.EmployeeProjects.Remove(employeeProject);

    public async Task<IReadOnlyCollection<EmployeeProjectResponseDto>> GetAllEmployeeProjectsAsync()
    {
        return await context.EmployeeProjects.AsNoTracking()
            .Include(x => x.User)
                .ThenInclude(x => x!.Role)
            .Include(x => x.Project)
            .Select(x => new EmployeeProjectResponseDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.User != null ? x.User.Name ?? "" : "",
                UserEmail = x.User != null ? x.User.Email ?? "" : "",
                RoleName = x.User != null && x.User.Role != null ? x.User.Role.Name : "",
                ProjectId = x.ProjectId,
                ProjectName = x.Project != null ? x.Project.Name : "",
                AssignedDate = x.AssignedDate
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> UserExistsAsync(Guid userId) => await context.Users.AnyAsync(x => x.Id == userId).ConfigureAwait(false);

    public async Task<int> GetUserProjectsCountAsync(Guid userId) => await context.EmployeeProjects.AsNoTracking().Where(x => x.UserId == userId).CountAsync().ConfigureAwait(false);

    public async Task<IReadOnlyCollection<ProjectResponseDto>> GetUserProjectsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        return await context.EmployeeProjects.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Project!.StartDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProjectResponseDto
            {
                Id = x.Project != null ? x.Project.Id : Guid.Empty,
                Name = x.Project != null ? x.Project.Name : "",
                Description = x.Project != null ? x.Project.Description : "",
                StartDate = x.Project != null ? x.Project.StartDate : DateTime.MinValue,
                EndDate = x.Project != null ? x.Project.EndDate : null,
                ProjectManagerId = x.Project != null ? x.Project.ProjectManagerId : Guid.Empty,
                ProjectManagerName = x.Project != null && x.Project.ProjectManager != null ? x.Project.ProjectManager.Name ?? "" : "",
                TotalUsers = context.EmployeeProjects.Count(ep => ep.ProjectId == x.ProjectId)
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
