using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Command
{
    public interface ICommand
    {
        string Raw { get; }
        string Command { get; }
        object[] Arguments { get; }
        IReadOnlyDictionary<string, object> Options { get; }

        bool ContainsError { get; }
        string ErrorMessage { get; }
    }
}
