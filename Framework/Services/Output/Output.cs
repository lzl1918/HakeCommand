using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.Output
{
    internal sealed class Output : IOutput
    {
        public void Clear()
        {
            Console.Clear();
        }

        public void OutputObject(object result)
        {
            if (result == null)
                return;

            Console.WriteLine(result);
        }

        public void WriteSplash()
        {
            Console.WriteLine("HakeCommand");
            Console.WriteLine();
        }

        public void WriteScopeBegin(IEnvironment env)
        {
            Console.Write($"{env.WorkingDirectory.FullName}> ");
        }
    }
}
