using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ShivOhm.Infrastructure
{
    public static class RegisterRootServices
    {
        public static void RootServicesRegistered(this IServiceCollection services, string MiratorConnectionStrings)
        {
            services.AddSingleton<ILog, LogNLog>();
            services
            .AddLogging(c => c.AddFluentMigratorConsole())
            .AddFluentMigratorCore()
            .ConfigureRunner(c => c
            .AddSqlServer()
            .WithGlobalConnectionString(MiratorConnectionStrings)
            .ScanIn(Assembly.GetEntryAssembly()).For.All());
        }
    }
}
