using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.OutputEngine;
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
        private IOutputEngine output;
        private HostInput hostInput;

        private AutoResetEvent mainblock;

        public Host(IServiceProvider services, IServiceCollection pool, IAppBuilder appBuilder,
            IEnvironment hostEnv, IOutputEngine output, IHostInput hostInput)
        {
            this.services = services;
            this.pool = pool;
            this.appBuilder = appBuilder;
            this.hostEnv = hostEnv;
            this.output = output;
            this.hostInput = hostInput as HostInput;
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
                output.WriteScopeBegin(hostEnv);
                string command = hostInput.ReadCommandLine();
                if (command == null)
                    break;
                IInputCollection inputCollection = InternalInput.Parse(command);
                if (inputCollection.ContainsError)
                {
                    output.WriteError(inputCollection.ErrorMessage);
                }
                else
                {
                    foreach (var desc in pool.GetDescriptors())
                        desc.EnterScope();

                    object inputObject = null;
                    foreach (IInput input in inputCollection.Inputs)
                    {
                        HostContext context = new HostContext(input, inputObject);
                        AppDelegate app = appBuilder.Build();
                        await app(context);
                        inputObject = context.Result;
                    }
                    output.WriteObject(inputObject);

                    foreach (var desc in pool.GetDescriptors())
                        desc.LeaveScope();
                }
            }
        }
    }
}
