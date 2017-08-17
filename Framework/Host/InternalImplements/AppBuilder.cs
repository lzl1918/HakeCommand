using HakeCommand.Framework.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HakeCommand.Framework.Host.InternalImplements
{
    internal sealed class AppBuilder : IAppBuilder
    {
        public IServiceProvider Services { get; }
        private List<Func<AppDelegate, AppDelegate>> components = new List<Func<AppDelegate, AppDelegate>>();

        public AppBuilder(IServiceProvider services)
        {
            Services = services;

            // block syntax errors
            this.Use(next =>
            {
                return context =>
                {
                    ICommand inputCommand = context.Command;
                    if (inputCommand.ContainsError)
                    {
                        context.SetResult(inputCommand.ErrorMessage);
                        return Task.CompletedTask;
                    }
                    else if (inputCommand.Command == null || inputCommand.Command.Length <= 0)
                        return Task.CompletedTask;
                    else
                        return next(context);
                };
            });
        }

        public AppDelegate Build()
        {
            AppDelegate app = context =>
            {
                ICommand inputCommand = context.Command;
                string result = $"command not found: {inputCommand.Command}";
                context.SetResult(result);
                return Task.CompletedTask;
            };
            foreach (Func<AppDelegate, AppDelegate> component in components)
                app = component(app);
            return app;
        }

        public IAppBuilder Use(Func<AppDelegate, AppDelegate> component)
        {
            components.Insert(0, component);
            return this;
        }
    }
}
