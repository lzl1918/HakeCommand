using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Framework.Services.OutputEngine
{

    public sealed class OutputHeader : IOutputHeader
    {
        internal OutputHeader(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
    public sealed class OutputBody : IOutputBody
    {
        internal OutputBody(List<string> contents)
        {
            Contents = contents;
        }

        public IReadOnlyList<string> Contents { get; }
    }
    public sealed class OutputTabFormatEnd : IOutputBody
    {
        internal OutputTabFormatEnd()
        {
        }

        public IReadOnlyList<string> Contents => throw new NotImplementedException();
    }
    public sealed class OutputLineSeperator : IOutputBody
    {
        internal OutputLineSeperator()
        {
        }

        public IReadOnlyList<string> Contents => throw new NotImplementedException();
    }
    public sealed class OutputColumnLineSeperator : IOutputBody
    {
        internal OutputColumnLineSeperator()
        {
        }

        public IReadOnlyList<string> Contents => throw new NotImplementedException();
    }
    public sealed class OutputFooter : IOutputFooter
    {
        internal OutputFooter(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
    public sealed class OutputInfo : IOutputInfo
    {
        private OutputInfo(IOutputHeader header, List<IOutputBody> body, IOutputFooter footer)
        {
            Header = header;
            Body = body;
            Footer = footer;
        }

        public IOutputHeader Header { get; }

        public IReadOnlyList<IOutputBody> Body { get; }

        public IOutputFooter Footer { get; }


        public static OutputInfo CreateEmpty()
        {
            return new OutputInfo(null, null, null);
        }
        public static OutputInfo Create(string header, IEnumerable<IEnumerable<string>> bodies, string footer)
        {
            OutputHeader outputHeader = null;
            if (header != null)
                outputHeader = new OutputHeader(header);
            List<IOutputBody> outputBodies = CreateBodies(bodies);
            OutputFooter outputFooter = null;
            if (footer != null)
                outputFooter = new OutputFooter(footer);
            return new OutputInfo(outputHeader, outputBodies, outputFooter);
        }
        public static OutputInfo Create(string header, IEnumerable<IOutputBody> bodies, string footer)
        {
            OutputHeader outputHeader = null;
            if (header != null)
                outputHeader = new OutputHeader(header);
            List<IOutputBody> outputBodies = null;
            if (bodies != null)
                outputBodies = new List<IOutputBody>(bodies);
            OutputFooter outputFooter = null;
            if (footer != null)
                outputFooter = new OutputFooter(footer);
            return new OutputInfo(outputHeader, outputBodies, outputFooter);
        }
        public static OutputInfo Create<T>(string header, IList<T> objects, IList<string> properties, string footer)
        {
            OutputHeader outputHeader = null;
            if (header != null)
                outputHeader = new OutputHeader(header);
            List<IOutputBody> outputBodies = RetriveProperties<T>(objects, properties, true);
            OutputFooter outputFooter = null;
            if (footer != null)
                outputFooter = new OutputFooter(footer);
            return new OutputInfo(outputHeader, outputBodies, outputFooter);
        }
        public static List<IOutputBody> RetriveProperties<T>(IList<T> objects, IList<string> properties, bool addTitle)
        {
            List<IOutputBody> outputBodies = null;
            if (objects != null && properties != null && objects.Count > 0 && properties.Count > 0)
            {
                List<string> propNames = new List<string>();
                List<MethodInfo> getMethods = new List<MethodInfo>();
                Type objectType = typeof(T);
                PropertyInfo propInfo;
                MethodInfo getMethod;
                foreach (string prop in properties)
                {
                    propInfo = objectType.GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null)
                        continue;
                    getMethod = propInfo.GetMethod;
                    if (getMethod == null)
                        continue;
                    propNames.Add(propInfo.Name);
                    getMethods.Add(getMethod);
                }
                if (propNames.Count > 0)
                {
                    outputBodies = new List<IOutputBody>();
                    List<string> contents;
                    OutputBody outBody;
                    if (addTitle)
                    {
                        contents = new List<string>(propNames);
                        outBody = new OutputBody(contents);
                        outputBodies.Add(outBody);
                        outputBodies.Add(CreateColumnLineSeperator());
                    }
                    object retValue;
                    foreach (T obj in objects)
                    {
                        contents = new List<string>();
                        foreach (MethodInfo method in getMethods)
                        {
                            retValue = method.Invoke(obj, null);
                            if (retValue == null)
                                contents.Add("");
                            else
                                contents.Add(retValue.ToString());
                        }
                        outBody = new OutputBody(contents);
                        outputBodies.Add(outBody);
                    }
                }
            }
            return outputBodies;
        }
        public static List<IOutputBody> CreateBodies(IEnumerable<IEnumerable<string>> bodies)
        {
            List<IOutputBody> outputBodies = null;
            if (bodies != null)
            {
                outputBodies = new List<IOutputBody>();
                foreach (IEnumerable<string> bodyContent in bodies)
                    outputBodies.Add(new OutputBody(new List<string>(bodyContent)));
            }
            return outputBodies;
        }
        public static IOutputBody CreateLineSeperator()
        {
            return new OutputLineSeperator();
        }
        public static IOutputBody CreateColumnLineSeperator()
        {
            return new OutputColumnLineSeperator();
        }
        public static IOutputBody CreateFormatEnd()
        {
            return new OutputTabFormatEnd();
        }
    }
}
