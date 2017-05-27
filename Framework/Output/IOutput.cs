using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Output
{
    public interface IOutput
    {
        void OutputObject(object result);
    }
}
