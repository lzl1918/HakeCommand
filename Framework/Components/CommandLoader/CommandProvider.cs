using HakeCommand.Framework.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace HakeCommand.Framework.Components.CommandLoader
{
    internal sealed class CommandProvider : ICommandProvider
    {
        private Dictionary<string, CommandRecord> commands;
        public IReadOnlyDictionary<string, CommandRecord> Commands { get { return commands; } }

        public CommandProvider()
        {
            commands = LoadCommands();
        }

        public CommandRecord MatchCommand(IHostContext context)
        {
            CommandRecord record;
            if (commands.TryGetValue(context.Command.Command, out record))
                return record;
            else
                return null;
        }

        private Dictionary<string, CommandRecord> LoadCommands()
        {
            Dictionary<string, CommandRecord> commands = new Dictionary<string, CommandRecord>();
            string dir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            FileInfo[] files = directoryInfo.GetFiles("*.dll");
            Type commandsetType = typeof(CommandSet);
            string typeName;
            string methodName;
            string command;
            IEnumerable<CommandAttribute> commandAttributes;
            foreach (FileInfo file in files)
            {
                Assembly assembly = Assembly.Load(AssemblyLoadContext.GetAssemblyName(file.FullName));
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    TypeInfo typeInfo = type.GetTypeInfo();
                    if (typeInfo.IsArray == true) continue;
                    if (typeInfo.IsEnum == true) continue;
                    if (typeInfo.IsAbstract == true) continue;
                    if (typeInfo.IsInterface == true) continue;
                    if (typeInfo.IsClass == false) continue;
                    if (!commandsetType.IsAssignableFrom(type)) continue;

                    typeName = type.Name;
                    if (typeName.EndsWith("Commands"))
                        typeName = typeName.Substring(0, typeName.Length - 8);
                    typeName = typeName.ToLower();
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        methodName = method.Name.ToLower();
                        commandAttributes = method.GetCustomAttributes<CommandAttribute>();
                        foreach (CommandAttribute commandAttribute in commandAttributes)
                        {
                            command = commandAttribute.Name.Trim().ToLower();
                            if (command.Length <= 0)
                                continue;
                            commands[command] = new CommandRecord(command, type, method);
                        }
                    }
                }
            }
            return commands;
        }


        #region IDisposable Support
        private bool disposedValue = false;


        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }
                commands = null;
                disposedValue = true;
            }
        }
        ~CommandProvider()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
