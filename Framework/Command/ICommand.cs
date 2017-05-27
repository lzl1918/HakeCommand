using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Command
{
    public interface ICommand
    {
        string Raw { get; }
        string Command { get; }
        string Action { get; }

        object[] UnnamedArguments { get; }
        IReadOnlyDictionary<string, object> NamedArguments { get; }

    }
}
