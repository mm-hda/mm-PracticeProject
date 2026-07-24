using backend.Data;
using backend.IRepository;

using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Repositories;

internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
