using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Template.Core.App.Usuarios.DataTransfer;
using Template.Core.App.Usuarios.Service;
using Template.Core.Domain.Usuarios.Enums;
using Template.Core.Domain.Usuarios.Service;

namespace Template.Core.IoC.Config.Auth;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(this IServiceProvider provider, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = provider.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        try
        {
            IUsuarioService usuarioService = services.GetRequiredService<IUsuarioService>();

            if (await usuarioService.ExisteAlgum(cancellationToken))
                return;

            string? login = configuration["AdminSeed:Login"];
            string? senha = configuration["AdminSeed:Senha"];
            string? email = configuration["AdminSeed:Email"];

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning("Nenhum usuário cadastrado e 'AdminSeed' (Login/Senha/Email) não configurado — admin inicial não criado.");
                return;
            }

            IUsuarioAppService usuarioAppService = services.GetRequiredService<IUsuarioAppService>();
            await usuarioAppService.RegistrarAsync(
                new UsuarioRegistrarRequest { Login = login, Senha = senha, Email = email, Role = UsuarioRoleEnum.ADMIN },
                cancellationToken);

            logger.LogInformation("Admin inicial '{Login}' criado a partir de AdminSeed.", login);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível semear o admin inicial. A migração ATOS-020 (tabela 'usuario') foi aplicada?");
        }
    }
}
