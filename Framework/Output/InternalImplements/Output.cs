using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Output.InternalImplements
{
    internal sealed class Output : IOutput
    {
        public void OutputObject(object result)
        {
            if (result == null)
                return;

            Console.WriteLine(result);
        }
    }
}
