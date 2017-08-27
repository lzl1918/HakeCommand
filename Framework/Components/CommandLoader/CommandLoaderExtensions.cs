using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Host;
using HakeCommand.Framework.Services.Environment;
using System;
using System.IO;
using System.Reflection;

namespace HakeCommand.Framework.Components.CommandLoader
{
    public static class CommandLoaderExtensions
    {
        private static readonly Type DIRECTORYINFO_TYPE = typeof(DirectoryInfo);
        private static readonly Type FILEINFO_TYPE = typeof(FileInfo);
        private static readonly Type STRING_TYPE = typeof(string);

        public static IServiceCollection AddCommandProvider(this IServiceCollection pool)
        {
            IServiceProvider services = pool.GetDescriptor<IServiceProvider>().GetInstance() as IServiceProvider;
            ICommandProvider provider = services.CreateInstance<CommandProvider>();
            pool.Add(ServiceDescriptor.Singleton<ICommandProvider>(provider));
            return pool;
        }

        private static IEnvironment environment;
        public static IAppBuilder UseCommands(this IAppBuilder builder)
        {
            environment = builder.Services.GetService<IEnvironment>();

            ObjectFactory.ValueMatching += OnValueMatching;

            return builder.UseComponent<CommandLoader>();
        }

        private static void OnValueMatching(object sender, ValueMatchingEventArgs e)
        {
            if (e.InputType == STRING_TYPE)
            {
                if (e.TargetType == DIRECTORYINFO_TYPE)
                {
                    if (e.InputValue == null)
                        e.SetValue(null);
                    else
                        e.SetValue(new DirectoryInfo(Path.Combine(environment.WorkingDirectory.FullName, e.InputValue as string)));
                }
                else if (e.TargetType == FILEINFO_TYPE)
                {
                    if (e.InputValue == null)
                        e.SetValue(null);
                    else
                        e.SetValue(new FileInfo(Path.Combine(environment.WorkingDirectory.FullName, e.InputValue as string)));
                }
            }
        }
    }
}
