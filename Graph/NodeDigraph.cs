using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    [Serializable]
    public class NodeDigraph<T> : IGraph<T>, IGraphTryGet<T, IEnumerable<T>>, IDigraphTryGet<T, IEnumerable<T>>
    {
        [Serializable]
        protected class Node
        {
            public HashSet<T> to = new HashSet<T>();
            public HashSet<T> from = new HashSet<T>();
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

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
                    if (from_node.to.Contains(to))
                    {

                        if (inner_table.ContainsKey(to))
                        {
                            return true;
                        }
                        else
                        {
                            from_node.to.Remove(to);
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
                from_node.to.Add(to);
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
                        from_node.to.Remove(to);
                        to_node.from.Remove(from);
                        return true;
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
                    tos = from_node.to;
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
                            from_node.to.Remove(item);
                        }
                    }
                    foreach (var to in node.to)
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
    public class MutualNodeGraph<T> : NodeDigraph<T>
    {
        public override void Set(T item1, T item2)
        {
            lock (WriteLock)
            {
                base.Set(item1, item2);
                base.Set(item2, item1);
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
    }
}
