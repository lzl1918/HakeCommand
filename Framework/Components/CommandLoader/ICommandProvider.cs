using HakeCommand.Framework.Host;
using System;
using System.Collections.Generic;

namespace HakeCommand.Framework.Components.CommandLoader
{
    internal interface ICommandProvider : IDisposable
    {
        CommandRecord MatchCommand(IHostContext context);

        IReadOnlyDictionary<string, CommandRecord> Commands { get; }
    }
}
