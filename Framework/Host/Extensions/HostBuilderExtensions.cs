using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Host
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureServices<T>(this IHostBuilder builder)
        {
            return builder.ConfigureServices((services) =>
            {
                object instance = services.CreateInstance<T>();
                ObjectFactory.InvokeMethod(instance, "ConfigureServices", services);
            });
        }
        public static IHostBuilder ConfigureComponents<T>(this IHostBuilder builder)
        {
            return builder.ConfigureServices((services) =>
            {
                object instance = services.CreateInstance<T>();
                ObjectFactory.InvokeMethod(instance, "ConfigureComponents", services);
            });
        }
        public static IHostBuilder UseConfiguration<T>(this IHostBuilder builder)
        {
            return builder.ConfigureServices((services) =>
            {
                object instance = services.CreateInstance<T>();
                ObjectFactory.InvokeMethod(instance, "ConfigureServices", services);
                ObjectFactory.InvokeMethod(instance, "ConfigureComponents", services);
            });
        }
    }
}
