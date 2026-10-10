namespace Template.Core.Infra.Settings;

public class PostgreSqlSettings
{
    public string Host { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SSLMode { get; set; } = string.Empty;
    public string ChannelBinding { get; set; } = string.Empty;
    public string Schema { get; set; } = "core";

    public string ConnectionString =>
        $"Host={Host};Database={Database};Username={Username};Password={Password};SSL Mode={SSLMode};Channel Binding={ChannelBinding}";
}
