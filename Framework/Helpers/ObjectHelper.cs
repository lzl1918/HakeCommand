using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Framework.Helpers
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
        private const int CACHE_SIZE = 20;
        public static bool IsEnumerable(object value)
        {
            Type type = value.GetType();
            return TypeHelper.IsEnumerable(type);
        }

        public static List<object> GetElements(object value)
        {
            Type type = value.GetType();
            if (!IsEnumerable(value))
                throw new Exception("can not retrive elements");
            IEnumerable enumerable = (IEnumerable)value;
            IEnumerator enumerator = enumerable.GetEnumerator();
            List<object> result = new List<object>();
            while (enumerator.MoveNext())
                result.Add(enumerator.Current);
            return result;
        }

        private static TypedCache<Dictionary<string, MethodInfo>> propertyEntryCache = new TypedCache<Dictionary<string, MethodInfo>>(CACHE_SIZE);
        private static Dictionary<string, MethodInfo> GetPropertyEntry(Type type)
        {
            Dictionary<string, MethodInfo> entry = new Dictionary<string, MethodInfo>();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string name;
            MethodInfo getMethod;
            foreach (PropertyInfo propertyInfo in properties)
            {
                name = propertyInfo.Name.ToLower();
                getMethod = propertyInfo.GetMethod;
                entry[name] = getMethod;
            }
            return entry;
        }
        public static object GetPropertyByName(object obj, string property, bool allowNull)
        {
            property = property.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("invalid property name", nameof(property));

            Type objType = obj.GetType();
            Dictionary<string, MethodInfo> propertyEntry;
            propertyEntryCache.TryGetItem(objType, out propertyEntry, GetPropertyEntry);
            if (propertyEntry.TryGetValue(property, out MethodInfo getMethod))
            {
                if (getMethod == null)
                {
                    if (allowNull)
                        return null;
                    throw new PropertyNotGetableException(property, objType);
                }
                else
                {
                    if (allowNull)
                    {
                        try
                        {
                            return getMethod.Invoke(obj, null);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    return getMethod.Invoke(obj, null);
                }
            }
            throw new PropertyNotFoundException(property, objType);
        }
        public static List<object> GetPropertiesByNames(object obj, IEnumerable<string> properties, bool allowNull)
        {
            Type objType = obj.GetType();
            Dictionary<string, MethodInfo> propertyEntry;
            propertyEntryCache.TryGetItem(objType, out propertyEntry, GetPropertyEntry);
            List<object> result = new List<object>();
            foreach (string property in properties)
            {
                if (propertyEntry.TryGetValue(property, out MethodInfo getMethod))
                {
                    if (getMethod != null)
                    {
                        if (allowNull)
                        {
                            try
                            {
                                result.Add(getMethod.Invoke(obj, null));
                            }
                            catch
                            {
                                result.Add(null);
                            }
                        }
                        else
                        {
                            result.Add(getMethod.Invoke(obj, null));
                        }
                    }
                    else if (allowNull)
                        result.Add(null);
                    else
                        throw new PropertyNotGetableException(property, objType);
                }
                else if (allowNull)
                    result.Add(null);
                else
                    throw new PropertyNotFoundException(property, objType);
            }
            return result;
        }

        private static TypedCache<MethodInfo> onGetValueEntryCache = new TypedCache<MethodInfo>(CACHE_SIZE);
        private static MethodInfo GetOnGetValueMethod(Type type) => type.GetMethod("OnGetValue", BindingFlags.Public | BindingFlags.Instance);
        public static object TryGetValue(object obj)
        {
            if (obj == null)
                return null;

            Type objectType = obj.GetType();
            MethodInfo onGetMethod;
            onGetValueEntryCache.TryGetItem(objectType, out onGetMethod, GetOnGetValueMethod);
            if (onGetMethod != null)
                obj = ObjectFactory.InvokeMethod(obj, onGetMethod);
            return obj;
        }
    }
}
