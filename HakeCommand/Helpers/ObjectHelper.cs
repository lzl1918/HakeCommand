using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Helpers
{
    public sealed class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(string propertyName, Type objType) : base($"can not find property {propertyName} of type {objType.Name}")
        {
        }
    }
    public sealed class PropertyNotGetableException : Exception
    {
        public PropertyNotGetableException(string propertyName, Type objType) : base($"can not get value of property {propertyName} of type {objType.Name}")
        {
        }
    }


    public static class ObjectHelper
    {
        public static bool IsEnumerable(object value)
        {
            Type type = value.GetType();
            Type enumType = type.GetInterface("System.Collections.IEnumerable");
            return enumType != null;
        }
        public static List<object> GetElements(object value)
        {
            Type type = value.GetType();
            Type enumType = type.GetInterface("System.Collections.IEnumerable");
            if (enumType == null)
                throw new Exception("can not retrive elements");
            MethodInfo getEnumeratorMethod = enumType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
            IEnumerator enumerator = (IEnumerator)getEnumeratorMethod.Invoke(value, null);
            List<object> result = new List<object>();
            while (enumerator.MoveNext())
                result.Add(enumerator.Current);
            return result;
        }
        public static object GetPropertyByName(object obj, string property)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            property = property.Trim();
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("message", nameof(property));

            Type objType = obj.GetType();
            PropertyInfo propertyInfo = objType.GetProperty(property);
            if (propertyInfo == null)
                throw new PropertyNotFoundException(property, objType);
            MethodInfo getMethod = propertyInfo.GetMethod;
            if (getMethod == null)
                throw new PropertyNotGetableException(property, objType);
            return getMethod.Invoke(obj, null);
        }
        public static object GetPropertyIgnoringCase(object obj, string property)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            property = property.Trim();
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("message", nameof(property));

            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Name.ToLower().Equals(property))
                {
                    MethodInfo getMethod = propertyInfo.GetMethod;
                    if (getMethod == null)
                        throw new PropertyNotGetableException(propertyInfo.Name, objType);
                    return getMethod.Invoke(obj, null);
                }
            }
            throw new PropertyNotFoundException(property, objType);
        }
        public static object TryGetValue(object obj)
        {
            Type objectType = obj.GetType();
            MethodInfo onGetMethod = objectType.GetMethod("OnGetValue", BindingFlags.Public | BindingFlags.Instance);
            if (onGetMethod != null)
                obj = ObjectFactory.InvokeMethod(obj, onGetMethod);
            return obj;
        }
    }
}
