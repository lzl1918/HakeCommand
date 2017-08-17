using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Host;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HakeCommand.Framework.Components.CommandLoader
{
    internal sealed class CommandLoader
    {
        private string GetHelpContent(ICommandProvider commandProvider)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var pair in commandProvider.Commands)
            {
                builder.AppendFormat("{0}\t{1}.{2}", pair.Key, pair.Value.InstanceType.Name, pair.Value.CommandEntry.Name);
                builder.AppendLine();
            }
            return builder.ToString();
        }
        public Task Invoke(IHostContext context, Func<Task> next, IServiceProvider services, ICommandProvider commandProvider)
        {
            if (context.Command.Command == "help")
            {
                context.SetResult(GetHelpContent(commandProvider));
                return Task.CompletedTask;
            }

            CommandRecord command = commandProvider.MatchCommand(context);
            if (command == null)
                return next();

            int length = context.Command.Arguments.Length;
            object[] parameters = new object[length + 1];
            parameters[0] = context;
            Array.Copy(context.Command.Arguments, 0, parameters, 1, length);

            try
            {
                object instance = ObjectFactory.CreateInstance(command.InstanceType, services, context.Command.Options, parameters);
                object value = ObjectFactory.InvokeMethod(instance, command.CommandEntry, services, context.Command.Options, parameters);
                if (command.CommandEntry.ReturnType != typeof(void))
                    context.SetResult(value);
            }
            catch (Exception ex)
            {
                context.SetResult(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}
