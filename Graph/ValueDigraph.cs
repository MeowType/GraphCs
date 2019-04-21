using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    [Serializable]
    public class ValueDigraph<T, V> : IValueGraph<T, V>, IGraphGet<T, IEnumerable<T>>, IDigraphGet<T, IEnumerable<T>>, IValueGraphGet<T, V, IEnumerable<V>>, IValueGraphSetIndex<T, V>
    {
        [Serializable]
        protected class Node
        {
            public ConcurrentDictionary<T, HashSet<V>> to = new ConcurrentDictionary<T, HashSet<V>>();
            public HashSet<T> from = new HashSet<T>();
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        V IValueGraphSetIndex<T, V>.this[T from, T to] { set => Set(from, to, value); }
        virtual public IEnumerable<V> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetTo(index, out var vals) ? vals : null;

        [NonSerialized]
        protected object WriteLock = new object();

        virtual public void Add(T item)
        {
            lock (WriteLock)
            {
                inner_table.GetOrAdd(item, _ => new Node());
            }
        }
        virtual protected Node GetOrAddNode(T item) => inner_table.GetOrAdd(item, _ => new Node());

        virtual public void Clear() => inner_table.Clear();

        virtual public void CopyTo(T[] array, int arrayIndex) => inner_table.Keys.CopyTo(array, arrayIndex);

        virtual public IEnumerator<T> GetEnumerator() => inner_table.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => inner_table.Keys.GetEnumerator();

        virtual public bool Contains(T item) => Has(item);
        virtual public bool Has(T item) => inner_table.ContainsKey(item);
        virtual public bool Has(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.ContainsKey(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            return true;
                        }
                        else
                        {
                            from_node.to.TryRemove(to, out var _);
                        }
                    }
                }
                return false;
            }
        }
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.TryGetValue(to, out var val_set))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (val_set.Contains(value))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            from_node.to.TryRemove(to, out var _);
                        }
                    }
                }
                return false;
            }
        }

        virtual public void Set(T from, T to, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                var val_set = from_node.to.GetOrAdd(to, i => new HashSet<V>());
                val_set.Add(value);
                to_node.from.Add(from);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        from_node.to.TryRemove(to, out var _);
                        to_node.from.Remove(from);
                        return true;
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.TryGetValue(to, out var val_set))
                    {
                        if (!inner_table.ContainsKey(to))
                        {
                            from_node.to.TryRemove(to, out var _);
                            return false;
                        }
                        return val_set.Remove(value);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetTo(T from, out IEnumerable<T> tos)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    tos = from_node.to.Keys;
                    return true;
                }
            }
            tos = null;
            return false;
        }
        virtual public bool TryGetFrom(T to, out IEnumerable<T> froms)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(to, out var to_node))
                {
                    froms = to_node.from;
                    return true;
                }
            }
            froms = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<V> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.TryGetValue(to, out var val_set))
                    {
                        values = val_set;
                        return true;
                    }
                }
                values = null;
                return false;
            }
        }

        virtual public bool Remove(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(item, out var node))
                {
                    foreach (var from in node.from)
                    {
                        if (inner_table.TryGetValue(from, out var from_node))
                        {
                            from_node.to.TryRemove(item, out var _);
                        }
                    }
                    foreach (var to in node.to.Keys)
                    {
                        if (inner_table.TryGetValue(to, out var to_node))
                        {
                            to_node.from.Remove(item);
                        }
                    }
                    return true;
                }
                return false;
            }
        }
    }

    [Serializable]
    public class MutualValueGraph<T, V> : ValueDigraph<T, V>
    {
        public override void Set(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                base.Set(item1, item2, value);
                base.Set(item2, item1, value);
            }
        }

        public override bool UnSet(T item1, T item2)
        {
            lock (WriteLock)
            {
                var ba = base.UnSet(item1, item2);
                var bb = base.UnSet(item2, item1);
                return ba || bb;
            }
        }

        public override bool UnSet(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                var ba = base.UnSet(item1, item2, value);
                var bb = base.UnSet(item2, item1, value);
                return ba || bb;
            }
        }
    }
}
