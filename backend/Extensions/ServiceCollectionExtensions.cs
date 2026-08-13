using backend.IService;
using backend.Services;
using backend.IRepository;
using backend.Repositories;
using backend.GenericRepositories;
namespace backend.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IEmployeeProjectService, EmployeeProjectService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IEmployeeProjectRepository, EmployeeProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICookieService, CookieService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
