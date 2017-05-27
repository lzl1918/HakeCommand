using System;
using System.Collections.Generic;
using System.Text;
using HakeCommand.Framework.Command;

namespace HakeCommand.Framework.Host.InternalImplements
{

    internal sealed class HostContext : IHostContext
    {
        public ICommand Command { get; }

        public object Result { get; private set; }

        public void SetResult(object result)
        {
            Result = result;
        }

        public HostContext(string command)
        {
            Command = Framework.Command.InternalImplements.Command.ParseCommand(command);
        }
    }
}
