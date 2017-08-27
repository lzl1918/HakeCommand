using System;
using System.Collections.Generic;

namespace HakeCommand.Framework.Helpers
{
    internal sealed class TypedCacheItem<T>
    {
        public TypedCacheItem(string typeFullName, T item)
        {
            TypeFullName = typeFullName;
            Item = item;
        }

        internal string TypeFullName { get; }
        internal T Item { get; }
    }
    public class TypedCache<T>
    {
        private int capacity;
        public int Capacity { get => capacity; }
        private LinkedList<TypedCacheItem<T>> items;

        public TypedCache(int capacity)
        {
            this.capacity = capacity;
            items = new LinkedList<TypedCacheItem<T>>();
        }

        public bool TryGetItem(Type type, out T item, Func<Type, T> fallback)
        {
            string fullName = type.FullName;
            var node = items.First;
            while (node != null)
            {
                if (node.Value.TypeFullName == fullName)
                {
                    item = node.Value.Item;
                    items.Remove(node);
                    items.AddFirst(node);
                    return true;
                }
                node = node.Next;
            }
            item = fallback(type);
            if (items.Count >= capacity)
                items.RemoveLast();
            items.AddFirst(new TypedCacheItem<T>(fullName, item));
            return false;
        }
    }
}
