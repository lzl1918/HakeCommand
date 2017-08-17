using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Host;
using System;

namespace HakeCommand.Framework.Components.CommandLoader
{
    public static class CommandLoaderExtensions
    {
        public static IServiceCollection AddCommandProvider(this IServiceCollection pool)
        {
            IServiceProvider services = pool.GetDescriptor<IServiceProvider>().GetInstance() as IServiceProvider;
            ICommandProvider provider = services.CreateInstance<CommandProvider>();
            pool.Add(ServiceDescriptor.Singleton<ICommandProvider>(provider));
            return pool;
        }
        public static IAppBuilder UseCommands(this IAppBuilder builder)
        {
            return builder.UseComponent<CommandLoader>();
        }
    }
}
