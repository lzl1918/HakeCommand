using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Host.InternalImplements;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HakeCommand.Framework.Host
{
    public sealed class HostBuilder : IHostBuilder
    {
        private IServiceProvider services;
        private IServiceCollection pool;
        private IAppBuilder appBuilder;

        public HostBuilder()
        {
            pool = Hake.Extension.DependencyInjection.Implementations.Implementation.CreateServiceCollection();
            services = Hake.Extension.DependencyInjection.Implementations.Implementation.CreateServiceProvider(pool);
            pool.Add(ServiceDescriptor.Singleton<IServiceProvider>(services));
            pool.Add(ServiceDescriptor.Singleton<IServiceCollection>(pool));

            appBuilder = services.CreateInstance<AppBuilder>();

            ConfigureInternalServices(pool);

        }

        public IHost Build()
        {
            return services.CreateInstance<InternalImplements.Host>();
        }

        public IHostBuilder ConfigureServices(Action<IServiceProvider> configureServices)
        {
            if (configureServices == null)
                throw new ArgumentNullException(nameof(configureServices));
            configureServices(services);
            return this;
        }

        private void ConfigureInternalServices(IServiceCollection pool)
        {
            pool.Add(ServiceDescriptor.Singleton<IAppBuilder>(appBuilder));
            pool.Add(ServiceDescriptor.Singleton<IOutput, Output>());
            pool.Add(ServiceDescriptor.Singleton<IEnvironment, Services.Environment.Environment>(services =>
            {
                return new Services.Environment.Environment("C:");
            }));
        }
    }
}
