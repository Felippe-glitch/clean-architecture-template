# `Infra` layer — Persistence

> Implements the `Domain`'s repository contracts on top of **EF Core** and maps entities to the
> database with the Fluent API (`IEntityTypeConfiguration<T>`). References `Domain`, `App`
> (for `IUnitOfWork`), and `CrossCutting`.

## Responsibility

- **Object-relational mapping** (`*Configuration`): table, columns, keys, conversions.
- **Concrete repositories** (`<Feature>Repository`) that inherit `GenericRepository<T>` and
  implement `Domain`'s `I<Feature>Repository` — they build the LINQ queries and delegate
  pagination/ordering to the generic base.
- The EF Core `DbContext` (`TemplateDbContext`) and the `IUnitOfWork` implementation
  (`Common/UnitOfWork.cs`, wrapping `SaveChangesAsync`).

## What lives here

```
Infra/
├─ TemplateDbContext.cs                     # DbSets + OnModelCreating (scans *Configuration)
├─ GenericRepository.cs                     # CRUD + pagination/ordering via reflection
├─ Common/UnitOfWork.cs                     # IUnitOfWork.CommitAsync → SaveChangesAsync
├─ Settings/PostgreSqlSettings.cs           # connection string bound from appsettings
└─ <Feature>/
   ├─ Mappers/<Feature>Configuration.cs     # IEntityTypeConfiguration<T> (Fluent API)
   └─ Repository/<Feature>Repository.cs
```

## Patterns (real examples)

### Mapping (EF Core Fluent API)
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(u => u.Login).HasColumnName("login");
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash");
        builder.Property(u => u.Role).HasColumnName("role").HasConversion<int>();
    }
}
```
- Tables/columns in **lowercase snake_case**.
- The `*Configuration` classes are discovered automatically by `TemplateDbContext.OnModelCreating`
  via `modelBuilder.ApplyConfigurationsFromAssembly(...)` — no need to register each one manually.
- There are no EF Core Migrations in this project yet; the schema is currently applied manually
  (see the `ATOS-020` reference in `AdminSeeder.cs`). Keep the physical table/columns in sync with
  each `*Configuration` by hand until migrations are introduced.

### Concrete repository
```csharp
public class UserRepository(TemplateDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public async Task<PaginatedResult<User>> FilterAsync(ListUsersFilter filters, int page, int pageSize, string sortBy, SortDirection sortDirection, CancellationToken ct)
    {
        IQueryable<User> query = _context.Users;
        if (filters.Active.HasValue) query = query.Where(u => u.Active == filters.Active.Value);
        return await ListAsync(query, page, pageSize, sortBy, sortDirection); // pagination/ordering in the generic base
    }
}
```
- Each filter is applied **conditionally** (only when set) on top of `_context.Set<T>()`.
- **Never** materialize before filtering (`.ToList()` only happens in the generic base, inside `ListAsync`).

### `GenericRepository<T>`
- Provides `GetAsync/InsertAsync/UpdateAsync/DeleteAsync` (over the `DbContext`) and
  `ListAsync(query, page, pageSize, sortBy, sortDirection)` with **dynamic ordering via
  reflection** (builds an `OrderBy(...)` `Expression` from the column name; falls back to `Id` if
  the field doesn't exist).
- The repository **doesn't commit**: `InsertAsync`/`UpdateAsync` only track the entity on the
  `DbContext`. Committing is the application service's job, via `IUnitOfWork.CommitAsync()`.

## Adding persistence for a feature

1. `Mappers/<Feature>Configuration.cs` — `IEntityTypeConfiguration<Entity>` with `ToTable`,
   `HasKey`, `Property`, relationships.
2. `Repository/<Feature>Repository.cs` — `: GenericRepository<Entity>(context), I<Feature>Repository`
   and implement `FilterAsync(...)`.
3. Register the repo in `IoC` (`AddInfra`) — the `*Configuration` is discovered on its own.
4. Make sure the table/columns exist in the actual database (manually, until migrations exist).

## Rules
- No business rules here: a repository only reads/writes. Business rules belong in `Domain`.
- Don't open a transaction here (that's `App`'s job, via `IUnitOfWork`).
