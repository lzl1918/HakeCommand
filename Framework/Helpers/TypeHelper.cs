using System;

namespace HakeCommand.Framework.Helpers
{
    public static class TypeHelper
    {
        private const int CACHE_SIZE = 20;
        private static TypedCache<bool> isEnumerableCache = new TypedCache<bool>(CACHE_SIZE);
        private static Type GetEnumerableType(Type type) => type.GetInterface("System.Collections.IEnumerable");
        private static bool GetIsEnumerable(Type type) => GetEnumerableType(type) != null;
        public static bool IsEnumerable(Type type)
        {
            bool result;
            isEnumerableCache.TryGetItem(type, out result, GetIsEnumerable);
            return result;
        }

        private static TypedCache<Type> enumerableElementTypeCache = new TypedCache<Type>(CACHE_SIZE);
        private static Type GetEnumerableElementTypeArgument(Type type)
        {
            Type enumType = type.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (enumType == null)
                return null;
            else
                return enumType.GetGenericArguments()[0];
        }
        public static Type GetEnumerableElementType(Type type)
        {
            Type elemType;
            enumerableElementTypeCache.TryGetItem(type, out elemType, GetEnumerableElementTypeArgument);
            return elemType;
        }
    }
}
