using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Host.InternalImplements
{

    internal sealed class HostContext : IHostContext
    {
        public IInput Command { get; }

        public object Result { get; private set; }

        public object InputObject { get; }

        public void SetResult(object result)
        {
            Result = result;
        }

        public HostContext(IInput command, object inputObject)
        {
            Command = command;
            InputObject = inputObject;
        }
    }
}
