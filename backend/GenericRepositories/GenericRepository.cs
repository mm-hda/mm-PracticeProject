using System.Linq.Expressions;

using backend.Data;

using Microsoft.EntityFrameworkCore;

namespace backend.GenericRepositories;

internal class GenericRepository<TEntity>(AppDbContext context)
    : IGenericRepository<TEntity>
    where TEntity : class
{
    protected AppDbContext Context { get; } = context;

    protected DbSet<TEntity> DbSet { get; } = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await DbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate is null)
        {
            return await DbSet.CountAsync(cancellationToken).ConfigureAwait(false);
        }
        return await DbSet.CountAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await DbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
     => await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        => await DbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);

    protected IQueryable<TEntity> Query() => DbSet.AsQueryable();

    protected IQueryable<TEntity> QueryAsNoTracking() => DbSet.AsNoTracking();

    public async Task<IReadOnlyCollection<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>> orderBy, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = QueryAsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query
            .OrderBy(orderBy)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>>? orderBy = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Where(predicate);

        if (orderBy is not null)
        {
            query = query.OrderBy(orderBy);
        }

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Update(TEntity entity, CancellationToken cancellationToken)
        => DbSet.Update(entity);

    public void Delete(TEntity entity, CancellationToken cancellationToken)
        => DbSet.Remove(entity);
    public void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        => DbSet.RemoveRange(entities);

}
