using NHibernate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Common;

public class UnitOfWork(ISession session) : IDisposable, IUnitOfWork
{
    private ITransaction transaction;
    
    public void BeginTransaction() => transaction = session.BeginTransaction();


    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (transaction is not null && transaction.IsActive) 
                await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (transaction is not null && transaction.IsActive) 
            transaction.Rollback();
    }
    
    public void Dispose()
    {
        transaction?.Dispose();
    }

}