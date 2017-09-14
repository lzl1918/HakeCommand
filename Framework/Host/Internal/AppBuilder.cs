using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hake.Extension.DependencyInjection.Abstraction;

namespace HakeCommand.Framework.Host.Internal
{
    internal sealed class AppBuilder : IAppBuilder
    {
        public IServiceProvider Services { get; }
        private List<Func<AppDelegate, AppDelegate>> components = new List<Func<AppDelegate, AppDelegate>>();

        public AppBuilder(IServiceProvider services)
        {
            Services = services;
        }

        public AppDelegate Build()
        {
            AppDelegate app = context =>
            {
                IOutputEngine outputEngine = Services.GetService<IOutputEngine>();
                IInput inputCommand = context.Command;
                string error = $"command not found: {inputCommand.Name}";
                outputEngine.WriteError(error);
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
