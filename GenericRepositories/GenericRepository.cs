using System.Linq.Expressions;

using backend.Data;

using Microsoft.EntityFrameworkCore;

namespace backend.GenericRepositories;

internal class GenericRepository<TEntity>(AppDbContext context) : IGenericRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; } = context;
    protected DbSet<TEntity> DbSet { get; } = context.Set<TEntity>();
    public async Task<TEntity?> GetByIdAsync(Guid id) => await DbSet.FindAsync([id]).ConfigureAwait(false);
    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync()
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }
    public async Task<IReadOnlyCollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync()
            .ConfigureAwait(false);
    }
    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate)
            .ConfigureAwait(false);
    }
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        if (predicate is null)
        {
            return await DbSet
                .CountAsync()
                .ConfigureAwait(false);
        }
        return await DbSet
            .CountAsync(predicate)
            .ConfigureAwait(false);
    }
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);
    }
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }
    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);
    }
    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Delete(TEntity entity) => DbSet.Remove(entity);

    public void DeleteRange(IEnumerable<TEntity> entities) => DbSet.RemoveRange(entities);

    protected IQueryable<TEntity> Query() => DbSet.AsQueryable();
    protected IQueryable<TEntity> QueryAsNoTracking() => DbSet.AsNoTracking();

}
