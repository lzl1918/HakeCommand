using HakeCommand.Framework;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using HakeCommand.Framework.Helpers;

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
    public sealed class PropertyInfo
    {
        public string Name { get; }
        public object Value { get; }

        public PropertyInfo(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> contents = new List<IEnumerable<string>>();
            contents.Add(new string[] { "Name", $": {Name}" });
            contents.Add(new string[] { "Value", $": {Value}" });
            return OutputInfo.Create(null, contents, "");
        }
    }
    public sealed class ObjectPropertySet
    {
        public IList<PropertyInfo> Properties { get; }

        public ObjectPropertySet(IList<PropertyInfo> properties)
        {
            Properties = properties;
        }

        public IList<PropertyInfo> OnGetValue() => Properties;
        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> contents = new List<IEnumerable<string>>();
            foreach (PropertyInfo prop in Properties)
                contents.Add(new string[] { $"{prop.Name}", $": {prop.Value}" });
            return OutputInfo.Create(null, contents, "");
        }
    }
    public sealed class PropertyInfoList
    {
        public IList<ObjectPropertySet> Objects { get; }

        public PropertyInfoList(IList<ObjectPropertySet> objects)
        {
            Objects = objects;
        }

        public IList<ObjectPropertySet> OnGetValue() => Objects;
        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> contents = new List<IEnumerable<string>>();
            if (Objects.Count > 0)
            {
                string[] properties = Objects[0].Properties.Select(p => p.Name).ToArray();
                contents.Add(properties);
                string[] values;
                foreach (ObjectPropertySet obj in Objects)
                {
                    values = obj.Properties.Select(p => p.Value?.ToString()).ToArray();
                    contents.Add(values);
                }
            }
            List<IOutputBody> bodies = OutputInfo.CreateBodies(contents);
            if (bodies.Count > 0)
                bodies.Insert(1, OutputInfo.CreateColumnLineSeperator());
            return OutputInfo.Create(null, bodies, "");
        }
    }

    public sealed class ObjectCommands : CommandSet
    {
        [Description("Get the property of a object by name")]
        [Command("select")]
        public object GetProperty(string[] properties)
        {
            object obj = Context.InputObject;
            if (obj == null)
                SetExceptionAndThrow(new Exception("no input object"));
            List<string> propertyList = new List<string>();
            foreach (string prop in properties)
            {
                if (!string.IsNullOrWhiteSpace(prop))
                    propertyList.Add(prop);
            }
            if (propertyList.Count <= 0)
            {
                string property = ReadLine("property name");
                if (string.IsNullOrWhiteSpace(property))
                    SetExceptionAndThrow(new Exception("invalid property name"));
                propertyList.Add(property);
            }
            if (ObjectHelper.IsEnumerable(obj))
            {
                try
                {
                    List<ObjectPropertySet> objects = new List<ObjectPropertySet>();
                    ObjectPropertySet objectProperty;
                    List<PropertyInfo> propertyValues;
                    List<object> values;
                    object instance;
                    foreach (object objInstance in ObjectHelper.GetElements(obj))
                    {
                        instance = ObjectHelper.TryGetValue(objInstance);
                        propertyValues = new List<PropertyInfo>();
                        values = ObjectHelper.GetPropertiesByNames(instance, propertyList, true);
                        for (int i = 0; i < values.Count; i++)
                            propertyValues.Add(new PropertyInfo(propertyList[i], values[i]));
                        objectProperty = new ObjectPropertySet(propertyValues);
                        objects.Add(objectProperty);
                    }
                    if (objects.Count > 1)
                        return new PropertyInfoList(objects);
                    else if (objects.Count == 1)
                    {
                        objectProperty = objects[0];
                        if (objectProperty.Properties.Count > 1)
                            return objectProperty;
                        else if (objectProperty.Properties.Count == 1)
                            return objectProperty.Properties[0];
                        else
                            return null;
                    }
                    else
                        return null;
                }
                catch (PropertyNotFoundException)
                {
                    List<object> values;
                    List<PropertyInfo> propertyValues = new List<PropertyInfo>();
                    values = ObjectHelper.GetPropertiesByNames(obj, propertyList, true);
                    for (int i = 0; i < values.Count; i++)
                        propertyValues.Add(new PropertyInfo(propertyList[i], values[i]));
                    if (propertyValues.Count > 1)
                        return new ObjectPropertySet(propertyValues);
                    else if (propertyValues.Count == 1)
                        return propertyValues[0];
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    SetExceptionAndThrow(ex);
                    throw;
                }

            }
            else
            {
                List<object> values;
                List<PropertyInfo> propertyValues = new List<PropertyInfo>();
                values = ObjectHelper.GetPropertiesByNames(obj, propertyList, true);
                for (int i = 0; i < values.Count; i++)
                    propertyValues.Add(new PropertyInfo(propertyList[i], values[i]));
                if (propertyValues.Count > 1)
                    return new ObjectPropertySet(propertyValues);
                else if (propertyValues.Count == 1)
                    return propertyValues[0];
                else
                    return null;
            }
        }

        [Description("Get the type name of a object")]
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
