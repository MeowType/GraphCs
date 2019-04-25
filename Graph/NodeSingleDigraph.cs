using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MeowType.Collections.Graph
{
    [Serializable]
    public class NodeSingleDigraphStruct<T> : IGraph<T>, ISingleGraphValueTryGet<T> where T : struct
    {
        [Serializable]
        protected class Node
        {
            public T? to = null;
            public T? from = null;
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public T? this[T index] => TryGetTo(index, out var val) ? val : null;

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
                    if (from_node.to!=null && from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            return true;
                        }
                        else
                        {
                            from_node.to = null;
                        }
                    }
                }
                return false;
            }
        }

        virtual public void Set(T from, T to)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                to_node.from = from;
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
                        from_node.to = null;
                        to_node.from = null;
                        return true;
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetTo(T from, out T? to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to != null)
                {
                    to = from_node.to;
                    return true;
                }
            }
            to = null;
            return false;
        }
        virtual public bool TryGetFrom(T to, out T? from)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(to, out var to_node) && to_node.from != null)
                {
                    from = to_node.from;
                    return true;
                }
            }
            from = null;
            return false;
        }

        virtual public bool Remove(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(item, out var node))
                {
                    if (node.from.HasValue)
                    {
                        if (inner_table.TryGetValue(node.from.Value, out var from_node))
                        {
                            from_node.to = null;
                        }
                    }
                    if (node.to.HasValue)
                    {
                        if (inner_table.TryGetValue(node.to.Value, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    return true;
                }
                return false;
            }
        }
    }
    [Serializable]
    public class NodeSingleDigraph<T> : IGraph<T>, ISingleGraphTryGet<T> where T : class
    {
        [Serializable]
        protected class Node
        {
            public T to = null;
            public T from = null;
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public T this[T index] => TryGetTo(index, out var val) ? val : default;

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
                    if (from_node.to != null && from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            return true;
                        }
                        else
                        {
                            from_node.to = null;
                        }
                    }
                }
                return false;
            }
        }

        virtual public void Set(T from, T to)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                to_node.from = from;
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
                        from_node.to = null;
                        to_node.from = null;
                        return true;
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetTo(T from, out T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to != null)
                {
                    to = from_node.to;
                    return true;
                }
            }
            to = null;
            return false;
        }
        virtual public bool TryGetFrom(T to, out T from)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(to, out var to_node) && to_node.from != null)
                {
                    from = to_node.from;
                    return true;
                }
            }
            from = null;
            return false;
        }

        virtual public bool Remove(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(item, out var node))
                {
                    if (node.from != null)
                    {
                        if (inner_table.TryGetValue(node.from, out var from_node))
                        {
                            from_node.to = null;
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
