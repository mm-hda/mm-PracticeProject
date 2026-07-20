using backend.IService;
using backend.Services;

namespace backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IEmployeeProjectService, EmployeeProjectService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
