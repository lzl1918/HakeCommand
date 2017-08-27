using HakeCommand.Framework;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.OutputEngine;
using HakeCommand.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class DisplayableTypeInfo
    {
        public Type Type { get; }
        public DisplayableTypeInfo(Type type)
        {
            Type = type;
        }
        public Type OnGetValue()
        {
            return Type;
        }
        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> bodyContents = new List<IEnumerable<string>>();
            bodyContents.Add(new string[] { "Name", $": {Type.Name}" });
            bodyContents.Add(new string[] { "Namespace", $": {Type.Namespace}" });
            bodyContents.Add(new string[] { "Assembly", $": {Type.Assembly.FullName}" });
            return OutputInfo.Create(null, bodyContents, "");
        }
    }

    public sealed class ObjectCommands : CommandSet
    {
        [Statement("Get the property of a object by name")]
        [Command("select")]
        public object GetProperty(string property)
        {
            object obj = Context.InputObject;
            if (obj == null)
                SetExceptionAndThrow(new Exception("no input object"));

            if (string.IsNullOrWhiteSpace(property))
                property = ReadLine("property name");
            if (ObjectHelper.IsEnumerable(obj))
            {
                try
                {
                    List<object> values = new List<object>();
                    foreach (object objInstance in ObjectHelper.GetElements(obj))
                        values.Add(ObjectHelper.GetPropertyIgnoringCase(objInstance, property));
                    return values;
                }
                catch (PropertyNotFoundException)
                {
                    return ObjectHelper.GetPropertyIgnoringCase(obj, property);
                }
                catch (Exception ex)
                {
                    SetExceptionAndThrow(ex);
                    throw;
                }

            }
            else
            {
                return ObjectHelper.GetPropertyIgnoringCase(obj, property);
            }
        }

        [Statement("Get the type name of a object")]
        [Command("type")]
        public DisplayableTypeInfo GetObjectType()
        {
            object obj = Context.InputObject;
            if (obj == null)
                SetExceptionAndThrow(new Exception("no input object"));
            return new DisplayableTypeInfo(obj.GetType());
        }
    }
}
