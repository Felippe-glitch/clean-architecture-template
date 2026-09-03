using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Template.Core.App.Users.DataTransfer;
using Template.Core.App.Users.Interfaces.Service;
using Template.Core.Domain.Users.Enums;
using Template.Core.Domain.Users.Interfaces.Service;

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
            IUserService userService = services.GetRequiredService<IUserService>();

            if (await userService.HasAny(cancellationToken))
                return;

            string? login = configuration["AdminSeed:Login"];
            string? password = configuration["AdminSeed:Password"];
            string? email = configuration["AdminSeed:Email"];

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning("No users registered and 'AdminSeed' (Login/Password/Email) not configured — initial admin not created.");
                return;
            }

            IUserAppService userAppService = services.GetRequiredService<IUserAppService>();
            await userAppService.RegisterAsync(
                new RegisterUserRequest { Login = login, Password = password, Email = email, Role = UserRole.ADMIN },
                cancellationToken);

            logger.LogInformation("Initial admin '{Login}' created from AdminSeed.", login);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not seed the initial admin. Has migration ATOS-020 (table 'users') been applied?");
        }
    }
}
