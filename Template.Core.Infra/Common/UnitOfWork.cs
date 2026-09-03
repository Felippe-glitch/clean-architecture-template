using Template.Core.App.Common;

namespace Template.Core.Infra.Common;

public class UnitOfWork(TemplateDbContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
