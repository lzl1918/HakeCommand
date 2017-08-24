using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Framework.Services.OutputEngine
{
    internal sealed class InternalOutputEngine : IOutputEngine
    {

        public InternalOutputEngine()
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void Clear()
        {
            Console.Clear();
        }

        public void WriteHint(string hint)
        {
            Console.Write(hint);
        }
        public void WriteObject(object obj)
        {
            if (obj == null)
                return;
            if (obj is string)
            {
                Console.WriteLine(obj);
                return;
            }

            Type interfaceType;
            TypeInfo objectType = obj.GetType().GetTypeInfo();
            if ((interfaceType = objectType.GetInterface("System.Collections.IEnumerable")) != null)
            {
                MethodInfo getEnumeratorMethod = interfaceType.GetMethod("GetEnumerator");
                object enumerator = getEnumeratorMethod.Invoke(obj, null);
                Type enumeratorType = enumerator.GetType();
                MethodInfo moveNextMethod = enumeratorType.GetMethod("MoveNext");
                MethodInfo currentMethod = enumeratorType.GetProperty("Current").GetMethod;
                bool moveNext;
                object current;
                while (true)
                {
                    moveNext = (bool)moveNextMethod.Invoke(enumerator, null);
                    if (!moveNext)
                        break;
                    current = currentMethod.Invoke(enumerator, null);
                    WriteObject(current);
                }
            }
            else
                Console.WriteLine(obj);
        }

        public void WriteSplash()
        {
            Console.WriteLine("HakeCommand");
            Console.WriteLine();
        }

        public void WriteScopeBegin(IEnvironment env)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{env.WorkingDirectory.FullName}> ");
            Console.ForegroundColor = color;
        }

        public void WriteError(string message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        public void WriteWarning(string message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }
    }
}
