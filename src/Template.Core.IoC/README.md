# `IoC` layer — Composition Root / DI

> *Composition root* that ties every layer together: registers services, repositories, EF Core,
> Mapster, CORS, JWT, and OpenAPI/Scalar. It's the only layer that knows about all the others.

## Responsibility

- Provide the **`IServiceCollection` extension methods** that the `Api` calls from `Program.cs`
  (`AddCommonServices`, `AddCustomRateLimiter`, etc.).
- Register **DI per context**: domain (`AddDomain`), application (`AddApplication`),
  infra/repositories + EF Core (`AddInfra`).
- Configure cross-cutting integrations: `Config/Cors`, `Config/Auth` (JWT + seeder),
  `Config/Database` (PostgreSQL), `Mapster`, `Scalar` (OpenAPI).

## What lives here

```
IoC/
├─ NativeInjectorBoostraper.cs       # AddCommonServices → AddDomain/AddApplication/AddInfra/...
├─ Config/Database/DatabaseConfig.cs # DbContext + Npgsql connection
├─ Config/Cors/CorsConfig.cs         # CORS policy (origins from env var)
├─ Config/Auth/{JwtConfig,AdminSeeder}.cs
├─ Config/Logging/LogConfig.cs       # Serilog (console + optional OTLP/Loki)
├─ Mapster/MapsterConfig.cs          # global TypeAdapterConfig + IMapper registration
├─ Scalar/…                          # OpenAPI/Bearer security for the docs
└─ Settings/…                        # configuration POCOs (bound from appsettings)
```

## Registration pattern

`AddCommonServices` wires everything together; each layer has its own method, with services/repos
registered as **`Scoped`** (per HTTP request), matching the `DbContext`'s own lifetime:

```csharp
public static IServiceCollection AddDomain(this IServiceCollection s) =>
    s.AddScoped<IUserService, UserService>()
     /* ...one per feature... */;

public static IServiceCollection AddApplication(this IServiceCollection s) =>
    s.AddSingleton<IPasswordHasher, BCryptPasswordHasher>()
     .AddScoped<IAuthAppService, AuthAppService>()
     .AddScoped<IUserAppService, UserAppService>() /* ... */;

public static IServiceCollection AddInfra(this IServiceCollection s, IConfiguration cfg, IHostEnvironment env) =>
    s.AddPostgreSqlContext(cfg, env)
     .AddScoped<IUserRepository, UserRepository>()
     .AddScoped<IUnitOfWork, UnitOfWork>();
```

## Adding a feature means registering 3 lines

1. `AddDomain` → `AddScoped<I<Feature>Service, <Feature>Service>()`.
2. `AddApplication` → `AddScoped<I<Feature>AppService, <Feature>AppService>()`.
3. `AddInfra` → `AddScoped<I<Feature>Repository, <Feature>Repository>()`.

> The `*Configuration` classes (Infra, `IEntityTypeConfiguration<T>`) are discovered automatically
> by `TemplateDbContext.OnModelCreating` via `ApplyConfigurationsFromAssembly` — no line-by-line
> registration needed.

## CORS via environment variable (`Config/Cors/CorsConfig.cs`)

> **Only matters in production.** `Program.cs` applies `UseCors` outside `Development`: in dev the
> Angular dev server proxies `/api` to this API (`frontend/proxy.conf.json`), so the browser sees
> everything on the same origin and there's no preflight. Setting `Cors:AllowedOrigins` in
> `appsettings.Development.json` has no effect. The policy is still **registered** in every
> environment (`AddCustomCors`) — registering without applying it is harmless.

Allowed origins (`Cors:AllowedOrigins`) accept **two formats**, useful for deployment:

- **Indexed array** (appsettings or env): `Cors__AllowedOrigins__0=https://a`,
  `Cors__AllowedOrigins__1=https://b`.
- **Single string** separated by `,`/`;` (fallback when the array is empty):
  `Cors__AllowedOriginsCsv=https://a,https://b` (or `Cors__AllowedOrigins=https://a,https://b`).

The trailing slash is stripped automatically (a CORS origin can't end in `/`). If **no** origin is
configured, the browser blocks cross-origin calls — check the env var in that environment.

## Rules
- Every new dependency gets registered here (otherwise DI fails at runtime).
- Secrets (DB, JWT) come from env vars/`appsettings.*` — **never** hardcode them.
