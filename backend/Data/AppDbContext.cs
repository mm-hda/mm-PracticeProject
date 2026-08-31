using Microsoft.EntityFrameworkCore;

using backend.Entities;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<EmployeeProject> EmployeeProjects => Set<EmployeeProject>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}
