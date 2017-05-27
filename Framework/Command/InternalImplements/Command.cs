using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Command.InternalImplements
{
    internal sealed class Command : ICommand
    {
        public string Raw { get; }

        public string Action { get; }

        public object[] UnnamedArguments { get; }

        public IReadOnlyDictionary<string, object> NamedArguments { get; }

        private string command;
        string ICommand.Command { get { return command; } }

        private Command(string raw, string command, string action, object[] unnamedArgs, IReadOnlyDictionary<string, object> namedArgs)
        {
            Raw = raw;
            this.command = command;
            Action = action;
            UnnamedArguments = unnamedArgs;
            NamedArguments = namedArgs;
        }

        internal static ICommand ParseCommand(string command)
        {
            string parsedAction = "";
            string parsedCommand = "";
            List<object> parsedUnnamedArgs = new List<object>();
            Dictionary<string, object> parsedNamedArgs = new Dictionary<string, object>();

            Command cmd = new Command(command, parsedCommand, parsedAction, parsedUnnamedArgs.ToArray(), parsedNamedArgs);

            return cmd;
        }
    }
}
