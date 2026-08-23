using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Common;

public interface IUnitOfWork
{
    void BeginTransaction();
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}