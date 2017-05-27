using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Output;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HakeCommand.Framework.Host.InternalImplements
{
    internal sealed class Host : IHost
    {
        private IServiceCollection pool;
        private IServiceProvider services;
        private IAppBuilder appBuilder;
        private IOutput output;

        private AutoResetEvent mainblock;

        public Host(IServiceProvider services, IServiceCollection pool, IAppBuilder appBuilder, IOutput output)
        {
            this.services = services;
            this.pool = pool;
            this.appBuilder = appBuilder;
            this.output = output;
        }

        public void Run()
        {
            mainblock = new AutoResetEvent(false);
            Task.Run(() =>
            {
                OnStart();
                mainblock.Set();
            });
            mainblock.WaitOne();
            mainblock.Dispose();
        }

        private async void OnStart()
        {
            while (true)
            {
                string command = Console.ReadLine();
                if (command == null)
                    break;
                HostContext context = new HostContext(command);
                AppDelegate app = appBuilder.Build();
                await app(context);
                output.OutputObject(context.Result);
            }
        }
    }
}
