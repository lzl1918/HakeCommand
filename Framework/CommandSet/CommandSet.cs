using HakeCommand.Framework.Host;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Runtime.ExceptionServices;

namespace HakeCommand.Framework
{
    public abstract class CommandSet
    {
        public IEnvironment Environment { get; internal set; }
        public IHostInput HostInput { get; internal set; }
        public IOutputEngine OutputEngine { get; internal set; }
        public IInput Command { get; internal set; }
        public IHostContext Context { get; internal set; }
        public object InputObject { get; internal set; }


        protected virtual void ReportWarning(string message)
        {
            OutputEngine.WriteWarning(message);
        }

        protected virtual void SetExceptionAndThrow(Exception ex)
        {
            Context.Exception = ex;
            ExceptionDispatchInfo.Throw(ex);
        }
        protected virtual void SetException(Exception ex)
        {
            Context.Exception = ex;
        }

        protected virtual string ReadLine(string hint)
        {
            OutputEngine.WriteHint(hint + ": ");
            return HostInput.ReadLine();
        }
    }
}
