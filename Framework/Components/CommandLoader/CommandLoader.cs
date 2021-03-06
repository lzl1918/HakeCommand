﻿using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Helpers;
using HakeCommand.Framework.Host;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HakeCommand.Framework.Components.CommandLoader
{
    public sealed class CommandParameterInfo
    {
        public string Name { get; }
        public bool HasDefault { get; }
        public string Type { get; }
        public object DefaultValue { get; }
        public string Description { get; }
        private CommandParameterInfo(string name, string type, object defaultValue, string description)
        {
            Name = name;
            Type = type;
            HasDefault = true;
            DefaultValue = defaultValue;
            Description = description;
        }
        private CommandParameterInfo(string name, string type, string description)
        {
            Name = name;
            Type = type;
            HasDefault = false;
            DefaultValue = null;
            Description = description;
        }

        public static CommandParameterInfo GetParameterInfo(ParameterInfo parameter)
        {
            string name = parameter.Name;
            string type = GetTypeString(parameter.ParameterType);
            string description = "";
            DescriptionAttribute descriptionAttribute = parameter.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                description = descriptionAttribute.Description;
            if (parameter.HasDefaultValue)
                return new CommandParameterInfo(name, type, parameter.DefaultValue, description);
            else
                return new CommandParameterInfo(name, type, description);
        }
        private static string GetTypeString(Type type)
        {
            Type elemType;
            if (type.Name == "String" && type.Namespace == "System")
                return "String";
            else if (type.IsArray)
                return GetTypeString(type.GetElementType()) + "[]";
            else if ((elemType = TypeHelper.GetEnumerableElementType(type)) != null)
                return GetTypeString(elemType) + "[]";
            else if (TypeHelper.IsEnumerable(type))
                return "Object[]";
            else
                return type.Name;
        }
    }
    public sealed class CommandInfo
    {
        private readonly static List<string> COMMAND_PARAMETER_PROPERTIES = new List<string>()
        {
            nameof(CommandParameterInfo.Name),
            nameof(CommandParameterInfo.Type),
            nameof(CommandParameterInfo.Description)
        };

        public string Command { get; }
        public string Description { get; }
        public List<CommandParameterInfo> Parameters { get; }

        private CommandInfo(string command, string description, List<CommandParameterInfo> parameters)
        {
            Command = command;
            Description = description;
            Parameters = parameters;
        }

        public static CommandInfo GetInfo(string command, MethodInfo method)
        {
            string commandDescription = method.Name;
            DescriptionAttribute descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                commandDescription = descriptionAttribute.Description;

            ParameterInfo[] parameters = method.GetParameters();
            List<CommandParameterInfo> parameterInfos = new List<CommandParameterInfo>();
            foreach (ParameterInfo parameter in parameters)
                parameterInfos.Add(CommandParameterInfo.GetParameterInfo(parameter));
            return new CommandInfo(command, commandDescription, parameterInfos);
        }

        public IOutputInfo OnWrite()
        {
            List<IOutputBody> bodies = new List<IOutputBody>();
            OutputBody body;
            body = new OutputBody(new List<string>(2) { "Command", $": {Command}" });
            bodies.Add(body);

            body = new OutputBody(new List<string>(2) { "Description", $": {Description}" });
            bodies.Add(body);

            if (Parameters.Count > 0)
            {
                body = new OutputBody(new List<string>(2) { "Parameters", ":" });
                bodies.Add(body);
                bodies.Add(OutputInfo.CreateFormatEnd());
                List<IOutputBody> outputParameters = OutputInfo.RetriveProperties<CommandParameterInfo>(Parameters, COMMAND_PARAMETER_PROPERTIES, true);
                bodies.AddRange(outputParameters);
            }
            return OutputInfo.Create(null, bodies, "");
        }
    }


    internal sealed class CommandLoader
    {
        private static readonly Type LIST_STRING_TYPE = typeof(List<string>);
        private static readonly Type LIST_DIRECTORYINFO_TYPE = typeof(List<DirectoryInfo>);
        private static readonly Type LIST_FILEINFO_TYPE = typeof(List<FileInfo>);

        private List<string> GetCommandsList(ICommandProvider commandProvider)
        {
            List<string> helps = new List<string>();
            string content;
            foreach (var pair in commandProvider.Commands)
            {
                content = string.Format("{0}\t{1}.{2}", pair.Key, pair.Value.InstanceType.Name, pair.Value.CommandEntry.Name);
                helps.Add(content);
            }
            return helps;
        }

        private Task ProcessWithHelp(IInput input, IHostContext context, ICommandProvider commandProvider)
        {
            string helpCommand = null;
            if (input.Options.TryGetValue("command", out object commandOption))
            {
                if (commandOption is string)
                    helpCommand = (string)commandOption;
                else if (commandOption is IEnumerable<string> commandEnum)
                    helpCommand = commandEnum.FirstOrDefault();
            }
            if (helpCommand == null)
            {
                foreach (object arg in input.Arguments)
                {
                    if (arg is string)
                    {
                        helpCommand = (string)arg;
                        break;
                    }
                    else if (arg is IEnumerable<string> argEnum)
                    {
                        helpCommand = argEnum.FirstOrDefault();
                        if (helpCommand != null)
                            break;
                    }
                }
            }
            if (helpCommand == null)
                context.SetResult(GetCommandsList(commandProvider));
            else
            {
                CommandRecord commandRecord = commandProvider.MatchCommandByName(helpCommand);
                if (commandRecord == null)
                {
                    Exception error = new Exception($"command not found: {helpCommand}");
                    context.Exception = error;
                }
                else
                {
                    string commandName = commandRecord.Command;
                    MethodInfo entryMethod = commandRecord.CommandEntry;
                    context.SetResult(CommandInfo.GetInfo(commandName, entryMethod));
                }
            }
            return Task.CompletedTask;
        }

        public Task Invoke(IHostContext context, Func<Task> next,
            IServiceProvider services, ICommandProvider commandProvider, IEnvironment env, IOutputEngine outputEngine, IHostInput hostInput)
        {
            IInput input = context.Command;
            if (input.Name == "help")
                return ProcessWithHelp(input, context, commandProvider);

            CommandRecord command = commandProvider.MatchCommand(context);
            if (command == null)
                return next();

            MethodInfo methodInfo = command.CommandEntry;
            ParameterInfo[] parameters = methodInfo.GetParameters();
            List<int> pathIndices = new List<int>();
            foreach (var parameter in parameters)
            {
                if (parameter.GetCustomAttribute<PathAttribute>() != null)
                    pathIndices.Add(parameter.Position);
            }

            try
            {
                CommandSet instance = (CommandSet)ObjectFactory.CreateInstance(command.InstanceType, services, input.Options, input.Arguments);
                instance.Command = input;
                instance.Context = context;
                instance.InputObject = context.InputObject;
                instance.OutputEngine = outputEngine;
                instance.HostInput = hostInput;
                instance.Environment = env;
                object value;
                if (pathIndices.Count <= 0)
                    value = ObjectFactory.InvokeMethod(instance, methodInfo, services, input.Options, input.Arguments);
                else
                {
                    MethodInvokeContext invokeContext = ObjectFactory.CreateInvokeContext(methodInfo, services, input.Options, input.Arguments);
                    string currentPath = env.WorkingDirectory.FullName;
                    ProcessPathAttribute(pathIndices, parameters, invokeContext.Arguments, currentPath);
                    value = invokeContext.Invoke(instance);
                }
                if (command.CommandEntry.ReturnType != typeof(void))
                    context.SetResult(value);
            }
            catch (Exception ex)
            {
                context.Exception = ex;
            }
            return Task.CompletedTask;
        }

        private void ProcessPathAttribute(IReadOnlyList<int> pathIndices, ParameterInfo[] parameters, object[] arguments, string currentPath)
        {
            TypeInfo parameterTypeInfo;
            object setValue;
            foreach (int index in pathIndices)
            {
                parameterTypeInfo = parameters[index].ParameterType.GetTypeInfo();
                setValue = arguments[index];
                if (setValue == null)
                    arguments[index] = GetDefaultPathValue(currentPath, parameterTypeInfo, false);
                else
                {
                    if (setValue is string strValue)
                    {
                        currentPath = Path.Combine(currentPath, strValue);
                        arguments[index] = GetDefaultPathValue(currentPath, parameterTypeInfo, true);
                    }
                    else if (setValue is DirectoryInfo dirInfo)
                        arguments[index] = GetDefaultPathValue(dirInfo.FullName, parameterTypeInfo, true);
                    else if (setValue is FileInfo fileInfo)
                        arguments[index] = GetDefaultPathValue(fileInfo.FullName, parameterTypeInfo, true);
                    else if (setValue is IEnumerable<string> strsValue)
                    {
                        if (strsValue.Count() <= 0)
                            arguments[index] = GetDefaultPathValue(currentPath, parameterTypeInfo, false);
                        else
                            arguments[index] = GetPathValues(strsValue, currentPath, parameterTypeInfo);
                    }
                    else if (setValue is IEnumerable<DirectoryInfo> dirsInfo)
                    {
                        if (dirsInfo.Count() <= 0)
                            arguments[index] = GetDefaultPathValue(currentPath, parameterTypeInfo, false);
                        else
                            arguments[index] = GetPathValues(dirsInfo, currentPath, parameterTypeInfo);
                    }
                    else if (setValue is IEnumerable<FileInfo> filesInfo)
                    {
                        if (filesInfo.Count() <= 0)
                            arguments[index] = GetDefaultPathValue(currentPath, parameterTypeInfo, false);
                        else
                            arguments[index] = GetPathValues(filesInfo, currentPath, parameterTypeInfo);
                    }
                }
            }
        }
        private object GetDefaultPathValue(string currentPath, TypeInfo targetType, bool useFile)
        {
            Type elementType;
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType();
                if (elementType.Name == "String" && elementType.Namespace == "System")
                    return new string[] { currentPath };

                if (elementType.Name == "DirectoryInfo" && elementType.Namespace == "System.IO")
                    return new DirectoryInfo[] { new DirectoryInfo(currentPath) };

                if (elementType.Name == "FileInfo" && elementType.Namespace == "System.IO")
                    return new FileInfo[0];
            }
            if (targetType.IsAssignableFrom(LIST_STRING_TYPE))
                return new List<string>() { currentPath };

            if (targetType.IsAssignableFrom(LIST_DIRECTORYINFO_TYPE))
                return new List<DirectoryInfo>() { new DirectoryInfo(currentPath) };

            if (targetType.IsAssignableFrom(LIST_FILEINFO_TYPE))
                return new List<FileInfo>();

            if (targetType.Name == "String" && targetType.Namespace == "System")
                return currentPath;

            if (targetType.Name == "DirectoryInfo" && targetType.Namespace == "System.IO")
                return new DirectoryInfo(currentPath);

            if (useFile && targetType.Name == "FileInfo" && targetType.Namespace == "System.IO")
                return new FileInfo(currentPath);
            return null;
        }
        private object GetPathValues(IEnumerable<string> paths, string currentPath, TypeInfo targetType)
        {
            Type elementType;
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType();
                if (elementType.Name == "String" && elementType.Namespace == "System")
                    return paths.Select(p => Path.Combine(currentPath, p)).ToArray();

                if (elementType.Name == "DirectoryInfo" && elementType.Namespace == "System.IO")
                    return paths.Select(p => new DirectoryInfo(Path.Combine(currentPath, p))).ToArray();

                if (elementType.Name == "FileInfo" && elementType.Namespace == "System.IO")
                    return paths.Select(p => new FileInfo(Path.Combine(currentPath, p))).ToArray();
            }
            if (targetType.IsAssignableFrom(LIST_STRING_TYPE))
                return paths.Select(p => Path.Combine(currentPath, p)).ToList();

            if (targetType.IsAssignableFrom(LIST_DIRECTORYINFO_TYPE))
                return paths.Select(p => new DirectoryInfo(Path.Combine(currentPath, p))).ToList();

            if (targetType.IsAssignableFrom(LIST_FILEINFO_TYPE))
                return paths.Select(p => new FileInfo(Path.Combine(currentPath, p))).ToList();

            if (targetType.Name == "String" && targetType.Namespace == "System")
                return Path.Combine(currentPath, paths.First());

            if (targetType.Name == "DirectoryInfo" && targetType.Namespace == "System.IO")
                return new DirectoryInfo(Path.Combine(currentPath, paths.First()));

            if (targetType.Name == "FileInfo" && targetType.Namespace == "System.IO")
                return new FileInfo(Path.Combine(currentPath, paths.First()));

            return null;
        }
        private object GetPathValues(IEnumerable<DirectoryInfo> paths, string currentPath, TypeInfo targetType)
        {
            Type elementType;
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType();
                if (elementType.Name == "String" && elementType.Namespace == "System")
                    return paths.Select(p => p.FullName).ToArray();

                if (elementType.Name == "DirectoryInfo" && elementType.Namespace == "System.IO")
                    return paths.ToArray();

                if (elementType.Name == "FileInfo" && elementType.Namespace == "System.IO")
                    return new FileInfo[0];
            }
            if (targetType.IsAssignableFrom(LIST_STRING_TYPE))
                return paths.Select(p => p.FullName).ToList();

            if (targetType.IsAssignableFrom(LIST_DIRECTORYINFO_TYPE))
                return paths.ToList();

            if (targetType.IsAssignableFrom(LIST_FILEINFO_TYPE))
                return new List<FileInfo>();

            if (targetType.Name == "String" && targetType.Namespace == "System")
                return paths.First().FullName;

            if (targetType.Name == "DirectoryInfo" && targetType.Namespace == "System.IO")
                return paths.First();

            return null;
        }
        private object GetPathValues(IEnumerable<FileInfo> paths, string currentPath, TypeInfo targetType)
        {
            Type elementType;
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType();
                if (elementType.Name == "String" && elementType.Namespace == "System")
                    return paths.Select(p => p.FullName).ToArray();

                if (elementType.Name == "DirectoryInfo" && elementType.Namespace == "System.IO")
                    return new DirectoryInfo[] { new DirectoryInfo(currentPath) };

                if (elementType.Name == "FileInfo" && elementType.Namespace == "System.IO")
                    return paths.ToArray();
            }
            if (targetType.IsAssignableFrom(LIST_STRING_TYPE))
                return paths.Select(p => p.FullName).ToList();

            if (targetType.IsAssignableFrom(LIST_DIRECTORYINFO_TYPE))
                return new List<DirectoryInfo>() { new DirectoryInfo(currentPath) };

            if (targetType.IsAssignableFrom(LIST_FILEINFO_TYPE))
                return paths.ToList();

            if (targetType.Name == "String" && targetType.Namespace == "System")
                return paths.First().FullName;

            if (targetType.Name == "DirectoryInfo" && targetType.Namespace == "System.IO")
                return currentPath;

            if (targetType.Name == "FileInfo" && targetType.Namespace == "System.IO")
                return paths.First();

            return null;
        }

    }
}
