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
        }

        public AppDelegate Build()
        {
            AppDelegate app = (context) =>
            {
                string result;
                if (context.Command.Action.Length <= 0)
                    result = $"{context.Command.Command} is not a command";
                else
                    result = $"{context.Command.Command}-{context.Command.Action} is not a command";

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
