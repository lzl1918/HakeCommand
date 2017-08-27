using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Framework.Services.OutputEngine
{
    internal sealed class InternalOutputEngine : IOutputEngine
    {
        private readonly IServiceProvider services;

        public InternalOutputEngine(IServiceProvider services)
        {
            Console.ForegroundColor = ConsoleColor.White;
            this.services = services;
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
            if (obj is IOutputInfo outputInfo)
            {
                WriteOutputInfo(outputInfo);
                return;
            }

            Type interfaceType;
            Type objectType = obj.GetType();
            MethodInfo onWriteMethod = objectType.GetMethod("OnWrite", BindingFlags.Public | BindingFlags.Instance);
            if (onWriteMethod != null)
            {
                object retVal = ObjectFactory.InvokeMethod(obj, onWriteMethod, services);
                if (retVal == null)
                    return;
                WriteObject(retVal);
                return;
            }
            else if ((interfaceType = objectType.GetInterface("System.Collections.IEnumerable")) != null)
            {
                MethodInfo getEnumeratorMethod = interfaceType.GetMethod("GetEnumerator");
                IEnumerator enumerator = (IEnumerator)getEnumeratorMethod.Invoke(obj, null);
                while (enumerator.MoveNext())
                    WriteObject(enumerator.Current);
            }
            else
                Console.WriteLine(obj);
        }
        private void WriteOutputInfo(IOutputInfo output)
        {
            if (output.Header != null && output.Header.Content != null)
                Console.WriteLine(output.Header.Content);
            if (output.Body != null)
                WriteBody(output.Body);
            if (output.Footer != null && output.Footer.Content != null)
                Console.WriteLine(output.Footer.Content);
        }
        private const int MAX_WIDTH = 15;
        private void WriteBody(IReadOnlyList<IOutputBody> body)
        {
            int maxWidth = Console.BufferWidth - 2;
            int columnCount = 0;
            List<int> columnSize = new List<int>();
            List<Dictionary<int, int>> sizeCounts = new List<Dictionary<int, int>>();
            List<int> indices = new List<int>();
            int index = 0;
            int start = 0;
            int bodyCount = body.Count;
            int end = bodyCount - 1;
            int count;
            int width;
            int times;
            int totalWidth;
            double proportion;
            Dictionary<int, int> sizeCount;
            IOutputBody current;
            IReadOnlyList<string> contents;
            int product;
            int maxproduct;
            StringBuilder writerBuilder = new StringBuilder();
            while (true)
            {
                columnCount = 0;
                columnSize.Clear();
                indices.Clear();
                sizeCounts.Clear();
                // find columnCount
                for (index = start; index < bodyCount; index++)
                {
                    current = body[index];
                    if (current is OutputBody)
                    {
                        contents = current.Contents;
                        count = contents.Count;
                        if (columnCount < count)
                            columnCount = count;
                        indices.Add(index);
                    }
                    else if (current is OutputTabFormatEnd)
                    {
                        end = index;
                        break;
                    }
                }
                for (int i = 0; i < columnCount; i++)
                    sizeCounts.Add(new Dictionary<int, int>());
                foreach (int i in indices)
                {
                    current = body[i];
                    contents = current.Contents;
                    count = contents.Count;
                    for (int j = 0; j < count; j++)
                    {
                        sizeCount = sizeCounts[j];
                        width = contents[j].Length;
                        if (sizeCount.ContainsKey(width))
                            sizeCount[width]++;
                        else
                            sizeCount[width] = 1;
                    }
                }
                totalWidth = 0;
                for (int i = 0; i < columnCount; i++)
                {
                    sizeCount = sizeCounts[i];
                    width = 0;
                    foreach (var pair in sizeCount)
                    {
                        if (pair.Key > width)
                            width = pair.Key;
                    }
                    totalWidth += width;
                    columnSize.Add(width);
                }
                if (totalWidth >= maxWidth)
                {
                    totalWidth = 0;
                    for (int i = 0; i < columnCount; i++)
                    {
                        sizeCount = sizeCounts[i];
                        width = 0;
                        times = 0;
                        maxproduct = 0;
                        foreach (var pair in sizeCount)
                        {
                            product = pair.Value * pair.Key;
                            if (product > maxproduct)
                            {
                                maxproduct = product;
                                times = pair.Value;
                                width = pair.Key;
                            }
                            else if (product == maxproduct && width < pair.Key)
                            {
                                width = pair.Key;
                                times = pair.Value;
                            }
                        }
                        totalWidth += width;
                        columnSize.Add(width);
                    }
                    for (int i = 0; i < columnCount; i++)
                    {
                        proportion = maxWidth / totalWidth;
                        width = columnSize[i];
                        if (width < MAX_WIDTH)
                        {
                            totalWidth -= width;
                            maxWidth -= columnSize[i];
                        }
                        else
                        {
                            columnSize[i] = Math.Max((int)(width * proportion), MAX_WIDTH);
                            totalWidth -= width;
                            maxWidth -= columnSize[i];
                        }
                        maxWidth--;
                    }
                }

                for (index = start; index <= end; index++)
                {
                    current = body[index];
                    if (current is OutputBody)
                    {
                        contents = current.Contents;
                        count = contents.Count;
                        for (int i = 0; i < count; i++)
                        {
                            StringOrSlice(writerBuilder, contents[i], columnSize[i]);
                        }
                        Console.WriteLine(writerBuilder.ToString());
                        writerBuilder.Clear();
                    }
                    else if (current is OutputLineSeperator)
                    {
                        for (int i = 0; i < maxWidth; i++)
                            Console.Write('-');
                        Console.WriteLine();
                        break;
                    }
                    else if (current is OutputColumnLineSeperator)
                    {
                        count = columnSize.Count;
                        for (int i = 0; i < count; i++)
                        {
                            width = columnSize[i];
                            for (int j = 0; j < width; j++)
                                writerBuilder.Append('-');
                            writerBuilder.Append(' ');
                        }
                        if (writerBuilder.Length > 0)
                            writerBuilder.Remove(writerBuilder.Length - 1, 1);
                        Console.WriteLine(writerBuilder.ToString());
                        writerBuilder.Clear();
                    }
                }

                end++;
                if (end >= bodyCount)
                    break;
                start = end;
                end = bodyCount - 1;
            }
        }
        private void StringOrSlice(StringBuilder builder, string content, int width)
        {
            int i = 0;
            if (content.Length <= width)
            {
                builder.Append(content);
                for (i = content.Length; i <= width; i++)
                    builder.Append(' ');
            }
            else
            {
                int widthM3 = width - 3;
                for (; i < widthM3; i++)
                    builder.Append(content[i]);
                builder.Append("... ");
            }
        }

        public void WriteSplash()
        {
            Console.WriteLine("HakeCommand");
            Console.WriteLine();
        }

        public void WriteScopeBegin(IEnvironment env)
        {
            Console.Write($"{env.WorkingDirectory.FullName}> ");
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
