using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.HistoryProvider;
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
        private IHistoryProvider historyProvider;

        private AutoResetEvent mainblock;

        public Host(IServiceProvider services, IServiceCollection pool, IAppBuilder appBuilder,
            IEnvironment hostEnv, IOutputEngine output, IHostInput hostInput, IHistoryProvider historyProvider)
        {
            this.services = services;
            this.pool = pool;
            this.appBuilder = appBuilder;
            this.hostEnv = hostEnv;
            this.output = output;
            this.hostInput = hostInput as HostInput;
            this.historyProvider = historyProvider;
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
                historyProvider.Reset();
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
                    int index = 0;
                    int changeIndex = inputCollection.Inputs.Count - 1;
                    bool isInPipe = inputCollection.Inputs.Count > 1;
                    bool writeDefaultValue = !isInPipe;
                    HostContext context = null;
                    foreach (IInput input in inputCollection.Inputs)
                    {
                        context = new HostContext(input, inputObject, isInPipe, index, writeDefaultValue);
                        AppDelegate app = appBuilder.Build();
                        await app(context);
                        if (context.WriteResult)
                            output.WriteObject(context.Result);
                        inputObject = context.Result;
                        index++;
                        if (index >= changeIndex)
                            writeDefaultValue = true;
                    }

                    foreach (var desc in pool.GetDescriptors())
                        desc.LeaveScope();
                }
                historyProvider.Add(inputCollection.Raw);
            }
        }
    }
}
