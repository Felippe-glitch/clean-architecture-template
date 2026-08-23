# Camada `Infra` — Persistência

> Implementa os contratos de repositório do `Domain` sobre **NHibernate** e mapeia as entidades
> para o banco com **FluentNHibernate**. Referencia só o `Domain`.

## Responsabilidade

- **Mapeamento objeto-relacional** (`*Map`/`*ClassMap`): tabela, colunas, chaves, relacionamentos.
- **Repositórios concretos** (`<Feature>Repository`) que herdam `GenericRepository<T>` e implementam
  `I<Feature>Repository` do `Domain` — montam as queries LINQ e delegam a paginação/ordenação ao
  genérico.
- Tipos utilitários de persistência (`Common/DateOnlyType.cs`).

## O que vive aqui

```
Infra/
├─ GenericRepository.cs                 # CRUD + paginação/ordenação por reflexão
├─ <Feature>/Mappers/<Feature>Map.cs    # FluentNHibernate ClassMap
└─ <Feature>/Repository/<Feature>Repository.cs
```

## Padrões (idiomas reais)

### Mapeamento (FluentNHibernate)
```csharp
public class DepartamentoMap : ClassMap<Departamento>
{
    public DepartamentoMap()
    {
        Table("departamento");
        Id(x => x.Id).Column("id").GeneratedBy.Identity();
        Map(x => x.Titulo).Column("titulo");
        References(x => x.Lider).Column("lider");          // muitos-p/-um (FK)
        HasMany(x => x.Eventos).KeyColumn("departamento").Inverse();  // um-p/-muitos
    }
}
```
- Tabelas/colunas em **snake_case minúsculo** (casam com o schema do Postgres — ver
  `../../migrations/ATOS` e `docs/DATABASE.md`).
- `References` = FK (lado “muitos”); `HasMany(...).Inverse()` = coleção (lado “um”).
- Os `*Map` são varridos automaticamente pelo `IoC` (`NHibernateConfig`); **não** precisa registrar
  cada um manualmente.

### Repositório concreto
```csharp
public class DepartamentoRepository(ISession Session)
    : GenericRepository<Departamento>(Session), IDepartamentoRepository
{
    public async Task<PaginatedResult<Departamento>> Filtrar(DepartamentoListarFilter filter, int pg, int qt, string cpOrd, TipoOrdenacao tpOrd)
    {
        IQueryable<Departamento> query = _session.Query<Departamento>();
        if (!string.IsNullOrWhiteSpace(filter.Titulo)) query = query.Where(x => x.Titulo.Contains(filter.Titulo));
        if (filter.Igreja.HasValue)                    query = query.Where(x => x.Igreja.Id == filter.Igreja.Value);
        return await ListarAsync(query, pg, qt, cpOrd, tpOrd);   // paginação/ordenação no genérico
    }
}
```
- Cada filtro é aplicado **condicionalmente** (só quando preenchido) sobre `_session.Query<T>()`.
- **Nunca** materialize antes de filtrar (`.ToList()` só no genérico, via `ListarAsync`).

### `GenericRepository<T>`
- Oferece `RecuperarAsync/InserirAsync/AtualizarAsync/DeletarAsync` (async da `ISession`) e
  `ListarAsync(query, pagina, qt, cpOrd, tpOrd)` com **ordenação dinâmica por reflexão** (monta a
  `Expression` de `OrderBy(...)` pelo nome da coluna; cai em `Id` se o campo não existir).

## Como adicionar persistência de uma feature

1. `Mappers/<Feature>Map.cs` — `ClassMap<Entidade>` com `Table`, `Id`, `Map`, `References`/`HasMany`.
2. `Repository/<Feature>Repository.cs` — `: GenericRepository<Entidade>(Session), I<Feature>Repository`
   e implemente `Filtrar(...)`.
3. Registre o repo no `IoC` (`AddInfra`) — o `*Map` é descoberto sozinho.
4. Garanta a tabela/colunas via **migração Liquibase** (`../../migrations/ATOS`).

## Regras
- Entidades mapeadas precisam de membros `virtual` e ctor `protected` (proxy do NHibernate) — isso é
  garantido no `Domain`.
- Não coloque regra de negócio aqui: repositório só busca/grava. Regra é do `Domain`.
- Não abra transação aqui (é da `App`/`UnitOfWork`).
