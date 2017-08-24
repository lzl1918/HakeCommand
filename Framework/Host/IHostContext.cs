﻿using Hake.Extension.Pipeline.Abstraction;

namespace HakeCommand.Framework.Host
{
    public interface IHostContext : IContext
    {
        IInput Command { get; }
        object Result { get; }
        object InputObject { get; }
        void SetResult(object result);
        bool InPipe { get; }
        int PipeIndex { get; }

        bool WriteResult { get; set; }
    }
}
