using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.Output
{
    public interface IOutput
    {
        void OutputObject(object result);

        void WriteScopeBegin(IEnvironment env);

        void WriteSplash();

        void Clear();
    }
}
