using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.OutputEngine
{
    public interface IOutputEngine
    {
        void WriteHint(string hint);
        void WriteObject(object result);
        void WriteError(string message);
        void WriteWarning(string message);

        void WriteScopeBegin(IEnvironment env);

        void WriteSplash();

        void Clear();
    }

    public static class OutputEngineExtensions
    {
        public static void WriteError(this IOutputEngine outputEngine, Exception ex)
        {
            outputEngine.WriteError(ex.Message);
        }
    }

}
