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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToContainer("Users");
            entity.HasPartitionKey(user => user.Id);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToContainer("Roles");
            entity.HasPartitionKey(role => role.Id);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToContainer("Branches");
            entity.HasPartitionKey(branch => branch.Id);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToContainer("Departments");
            entity.HasPartitionKey(department => department.Id);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToContainer("Positions");
            entity.HasPartitionKey(position => position.Id);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToContainer("Projects");
            entity.HasPartitionKey(project => project.Id);
        });

        modelBuilder.Entity<EmployeeProject>(entity =>
        {
            entity.ToContainer("EmployeeProjects");
            entity.HasPartitionKey(employeeProject => employeeProject.Id);
        });
    }
}
