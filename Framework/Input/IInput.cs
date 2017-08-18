using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework
{
    public interface IInput
    {
        string Name { get; }
        object[] Arguments { get; }
        IReadOnlyDictionary<string, object> Options { get; }
    }
}
