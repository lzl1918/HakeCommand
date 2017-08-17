using System;
using System.Reflection;

namespace HakeCommand.Framework.Components.CommandLoader
{
    internal sealed class CommandRecord
    {
        public CommandRecord(string command, Type instanceType, MethodInfo commandEntry)
        {
            Command = command;
            InstanceType = instanceType;
            CommandEntry = commandEntry;
        }

        public string Command { get; }
        public Type InstanceType { get; }
        public MethodInfo CommandEntry { get; }
    }

}
