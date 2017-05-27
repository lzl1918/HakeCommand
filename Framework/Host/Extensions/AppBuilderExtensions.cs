using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HakeCommand.Framework.Host
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder Use(this IAppBuilder builder, Func<IHostContext, Func<Task>, Task> component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            return builder.Use(next =>
            {
                return context =>
                {
                    Func<Task> callnext = () => next(context);
                    return component(context, callnext);
                };
            });
        }
        public static IAppBuilder UseComponent<T>(this IAppBuilder builder)
        {
            Type type = typeof(T);
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsAbstract == true)
                throw new InvalidOperationException($"cannot use an abstract class {type.FullName} as component");
            if (typeInfo.IsInterface == true)
                throw new InvalidOperationException($"cannot use an interface {type.FullName} as component");

            MethodInfo method = type.GetMethod("Invoke");
            if (method == null)
                throw new InvalidOperationException($"component does not contains any function named Invoke");
            if (method.ReturnType != typeof(Task))
                throw new InvalidOperationException($"function Invoke has invalid return type");

            return builder.Use((context, next) =>
            {
                IServiceProvider services = builder.Services;
                object instance = services.CreateInstance(type, next);
                object result = ObjectFactory.InvokeMethod(instance, method, services, context, next);
                if (result != null && result is Task)
                    return result as Task;
                return next();
            });
        }
    }
}
