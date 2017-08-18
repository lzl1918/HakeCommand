using HakeCommand.Framework.Host;
using HakeCommand.Framework.Services.OutputEngine;
using System;

namespace HakeCommand.Framework
{
    public abstract class CommandSet
    {
        public IOutputEngine OutputEngine { get; internal set; }
        public IInput Command { get; internal set; }
        public IHostContext Context { get; internal set; }
        public object InputObject { get; internal set; }

        protected virtual void ReportWarning(string message)
        {
            OutputEngine.WriteWarning(message);
        }
    }
}
