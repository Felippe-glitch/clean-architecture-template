# Camada `IoC` — Composition Root / DI

> *Composition root* que amarra todas as camadas: registra services, repositórios, NHibernate,
> Mapster, CORS, JWT, Cloudinary e OpenAPI/Scalar. É a única camada que conhece todas as outras.

## Responsabilidade

- Prover os **extension methods** de `IServiceCollection` que a `Api` chama em `Program.cs`
  (`AddCommonServices`, `AddCustomRateLimiter`, etc.).
- Registrar o **DI por contexto**: domínio (`AddDomain`), aplicação (`AddApplication`),
  infra/repositórios + NHibernate (`AddInfra`).
- Configurar integrações transversais: `Config/Cors`, `Config/Auth` (JWT + seeder),
  `Config/Cloudinary`, `Config/NHibernate`, `Mapster`, `Scalar` (OpenAPI).

## O que vive aqui

```
IoC/
├─ NativeInjectorBoostraper.cs        # AddCommonServices → AddDomain/AddApplication/AddInfra/...
├─ Config/NHibernate/NHibernateConfig.cs  # ISession + varredura dos *Map
├─ Config/Cors/CorsConfig.cs          # política de CORS (origens por env var)
├─ Config/Auth/{JwtConfig,AdminSeeder}.cs
├─ Config/Cloudinary/CloudinaryConfig.cs
├─ Mapster/MapsterConfig.cs           # aplica os IRegister (profiles) da App
├─ Scalar/…                           # OpenAPI/segurança da doc
└─ Settings/…                         # POCOs de configuração (bind do appsettings)
```

## Padrão de registro

`AddCommonServices` compõe tudo; cada camada tem seu método com **tempo de vida `Scoped`**
(requisição HTTP) para services/repos, e o `ISession`/`UnitOfWork` também por requisição:

```csharp
public static IServiceCollection AddDomain(this IServiceCollection s) =>
    s.AddScoped<IDepartamentoService, DepartamentoService>()
     /* ...um por feature... */;

public static IServiceCollection AddApplication(this IServiceCollection s) =>
    s.AddScoped<IUnitOfWork, UnitOfWork>()
     .AddSingleton<IPasswordHasher, BCryptPasswordHasher>()
     .AddScoped<IDepartamentoAppService, DepartamentoAppService>() /* ... */;

public static IServiceCollection AddInfra(this IServiceCollection s, IConfiguration cfg) =>
    s.AddPostgreSqlContext(cfg)
     .AddScoped<IDepartamentoRepository, DepartamentoRepository>() /* ... */;
```

## Ao adicionar uma feature, registre 3 linhas

1. `AddDomain` → `AddScoped<I<Feature>Service, <Feature>Service>()`.
2. `AddApplication` → `AddScoped<I<Feature>AppService, <Feature>AppService>()`.
3. `AddInfra` → `AddScoped<I<Feature>Repository, <Feature>Repository>()`.

> Os `*Map` (Infra) e os `*Profile` (App) são descobertos por varredura (`NHibernateConfig` /
> `MapsterConfig`) — não precisam de registro linha a linha.

## CORS por variável de ambiente (`Config/Cors/CorsConfig.cs`)

> **Só vale em produção.** `Program.cs` aplica `UseCors` apenas fora de `Development`: no dev o
> servidor do Angular faz proxy de `/api` para esta API (`frontend/proxy.conf.json`), então o browser
> vê tudo na mesma origem e não há preflight. Configurar `Cors:AllowedOrigins` em
> `appsettings.Development.json` não tem efeito. A policy continua sendo **registrada** em todos os
> ambientes (`AddCustomCors`) — registrar sem aplicar é inócuo.

As origens permitidas (`Cors:AllowedOrigins`) aceitam **dois formatos**, úteis para deploy:

- **Array indexado** (appsettings ou env): `Cors__AllowedOrigins__0=https://a`,
  `Cors__AllowedOrigins__1=https://b`.
- **String única** separada por `,`/`;` (fallback quando o array vem vazio):
  `Cors__AllowedOriginsCsv=https://a,https://b` (ou `Cors__AllowedOrigins=https://a,https://b`).

A barra final é removida automaticamente (origem CORS não pode terminar em `/`). Se **nenhuma**
origem for configurada, o browser bloqueia chamadas cross-origin — confira a env var no ambiente.

## Regras
- Toda dependência nova entra aqui (senão o DI falha em runtime).
- Segredos (DB, JWT, Cloudinary) vêm de env var/`appsettings.*` — **nunca** hardcode.
