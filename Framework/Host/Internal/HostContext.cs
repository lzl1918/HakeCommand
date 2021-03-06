﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Host.Internal
{

    internal sealed class HostContext : IHostContext
    {
        public IInput Command { get; }

        public object Result { get; private set; }

        public object InputObject { get; }

        public bool InPipe { get; }

        public int PipeIndex { get; }

        public bool WriteResult { get; set; }
        public Exception Exception { get; set; }

        public bool ErrorOccured { get { return Exception != null; } }

        public void SetResult(object result)
        {
            Result = result;
        }

        public HostContext(IInput command, object inputObject, bool inPipe, int pipeIndex, bool writeResultDefaultValue)
        {
            Command = command;
            InputObject = inputObject;
            InPipe = inPipe;
            PipeIndex = pipeIndex;
            WriteResult = writeResultDefaultValue;
        }
    }
}
