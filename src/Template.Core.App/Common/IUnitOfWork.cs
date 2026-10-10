using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Common;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
