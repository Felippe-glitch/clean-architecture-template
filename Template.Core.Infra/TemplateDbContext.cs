using Microsoft.EntityFrameworkCore;

using Template.Core.Domain.Users.Entity;
using Template.Core.Infra.Settings;

namespace Template.Core.Infra;

public class TemplateDbContext(DbContextOptions<TemplateDbContext> options, PostgreSqlSettings? settings = null) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(settings?.Schema))
            modelBuilder.HasDefaultSchema(settings.Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplateDbContext).Assembly);
    }
}
