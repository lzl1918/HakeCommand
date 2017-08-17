using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.Output;
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
        private IEnvironment hostEnv;
        private IOutput output;

        private AutoResetEvent mainblock;

        public Host(IServiceProvider services, IServiceCollection pool, IAppBuilder appBuilder,
            IEnvironment hostEnv, IOutput output)
        {
            this.services = services;
            this.pool = pool;
            this.appBuilder = appBuilder;
            this.hostEnv = hostEnv;
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
            output.WriteSplash();
            while (true)
            {
                foreach (var desc in pool.GetDescriptors())
                    desc.EnterScope();

                output.WriteScopeBegin(hostEnv);
                string command = Console.ReadLine();
                if (command == null)
                    break;
                HostContext context = new HostContext(command);
                AppDelegate app = appBuilder.Build();
                await app(context);
                output.OutputObject(context.Result);

                foreach (var desc in pool.GetDescriptors())
                    desc.LeaveScope();
            }
        }
    }
}
