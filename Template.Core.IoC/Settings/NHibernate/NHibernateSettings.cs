namespace Template.Core.IoC.Settings.NHibernate;

public class NHibernateSettings
{
    public string Host { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public virtual string ConnectionString => $"Host={Host};Database={Database};Username={Username};Password={Password}";
}

public class NHibernateSettingsPostgres : NHibernateSettings
{
    public string SSLMode { get; set; } = string.Empty;
    public string ChannelBinding { get; set; } = string.Empty;

    // As migrações Liquibase aplicam o schema em "core" (--default-schema-name=core).
    // Os mapeamentos usam nomes de tabela sem schema; este valor é usado como
    // default_schema do NHibernate (ver NHibernateConfig), que qualifica as tabelas
    // como "core.<tabela>" no SQL gerado. Não usamos "Search Path" na connection
    // string porque o pooler (PgBouncer) do Neon ignora esse startup parameter.
    public string Schema { get; set; } = "core";

    public override string ConnectionString => $"Host={Host};Database={Database};Username={Username};Password={Password};SSL Mode={SSLMode}; Channel Binding={ChannelBinding}";
}