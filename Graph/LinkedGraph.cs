using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MeowType.Collections.Graph
{
    
    [Serializable]
    public class LinkedGraph<T, V>: ILinkedGraph<T, V>, ILinkedGraphHasLink<T, V>, ILinkedGraphLink<T, V>, ILinkedGraphUnLink<T, V>, ILinkedGraphTryGet<T, V>, ILinkedGraphTryGetLinkValue<T, V>, ILinkedGraphNextLast<T, V>, ILinkedGraphTryNextLast<T, V>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphTryGet<T, V>, IDataGraphUnSet<T, V>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : class where V : class
    {
        protected class Node
        {
            public ConcurrentDictionary<T, HashSet<V>> bind = new ConcurrentDictionary<T, HashSet<V>>();
            public T Last { get; set; }
            public T Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), V> link_table = new ConcurrentDictionary<(T, T), V>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<V> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Contains(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                var val_set = from_node.bind.GetOrAdd(to, i => new HashSet<V>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.Add(value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t= to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Remove(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<V> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    if (node.Next != null && inner_table.TryGetValue(node.Next, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                if(link_table.TryGetValue((item1, item2), out var val))
                {
                    if (val.Equals(value))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        virtual public void Link(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                link_table.AddOrUpdate((item1, item2), value, (_t, _v) => value);
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if(link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue(T item1, T item2, out V value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    value = val;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public T Next(T item)
        {
            lock (WriteLock)
            {
                if(inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }

    [Serializable]
    public class LinkedGraphValue<T, V> : ILinkedGraph<T, V>, ILinkedGraphHasLink<T, V>, ILinkedGraphLink<T, V>, ILinkedGraphUnLink<T, V>, ILinkedGraphTryGet<T, V>, ILinkedGraphValueTryGetLinkValue<T, V>, ILinkedGraphNextLast<T, V>, ILinkedGraphTryNextLast<T, V>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphTryGet<T, V>, IDataGraphUnSet<T, V>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : class where V : struct
    {
        protected class Node
        {
            public ConcurrentDictionary<T, HashSet<V>> bind = new ConcurrentDictionary<T, HashSet<V>>();
            public T Last { get; set; }
            public T Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), V?> link_table = new ConcurrentDictionary<(T, T), V?>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<V> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Contains(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                var val_set = from_node.bind.GetOrAdd(to, i => new HashSet<V>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.Add(value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Remove(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<V> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    if (node.Next != null && inner_table.TryGetValue(node.Next, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    if (val.Equals(value))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        virtual public void Link(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                link_table.AddOrUpdate((item1, item2), value, (_t, _v) => value);
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue(T item1, T item2, out V? value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    value = val;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public T Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }

    [Serializable]
    public class ValueLinkedGraph<T, V> : ILinkedGraph<T, V>, ILinkedGraphHasLink<T, V>, ILinkedGraphLink<T, V>, ILinkedGraphUnLink<T, V>, ILinkedGraphTryGet<T, V>, ILinkedGraphTryGetLinkValue<T, V>, IValueLinkedGraphNextLast<T, V>, IValueLinkedGraphTryNextLast<T, V>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphTryGet<T, V>, IDataGraphUnSet<T, V>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : struct where V : class
    {
        protected class Node
        {
            public ConcurrentDictionary<T, HashSet<V>> bind = new ConcurrentDictionary<T, HashSet<V>>();
            public T? Last { get; set; }
            public T? Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), V> link_table = new ConcurrentDictionary<(T, T), V>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<V> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Contains(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                var val_set = from_node.bind.GetOrAdd(to, i => new HashSet<V>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.Add(value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Remove(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<V> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    if (node.Next != null && inner_table.TryGetValue(node.Next.Value, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last.Value, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    if (val.Equals(value))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        virtual public void Link(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                link_table.AddOrUpdate((item1, item2), value, (_t, _v) => value);
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue(T item1, T item2, out V value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    value = val;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public T? Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T? Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T? next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T? last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }

    [Serializable]
    public class ValueLinkedGraphValue<T, V> : ILinkedGraph<T, V>, ILinkedGraphHasLink<T, V>, ILinkedGraphLink<T, V>, ILinkedGraphUnLink<T, V>, ILinkedGraphTryGet<T, V>, ILinkedGraphValueTryGetLinkValue<T, V>, IValueLinkedGraphNextLast<T, V>, IValueLinkedGraphTryNextLast<T, V>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphTryGet<T, V>, IDataGraphUnSet<T, V>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : struct where V : struct
    {
        protected class Node
        {
            public ConcurrentDictionary<T, HashSet<V>> bind = new ConcurrentDictionary<T, HashSet<V>>();
            public T? Last { get; set; }
            public T? Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), V?> link_table = new ConcurrentDictionary<(T, T), V?>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<V> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Contains(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                var val_set = from_node.bind.GetOrAdd(to, i => new HashSet<V>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.Add(value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.Remove(value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<V> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    if (node.Next != null && inner_table.TryGetValue(node.Next.Value, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last.Value, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    if (val.Equals(value))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        virtual public void Link(T item1, T item2, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                link_table.AddOrUpdate((item1, item2), value, (_t, _v) => value);
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue(T item1, T item2, out V? value)
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var val))
                {
                    value = val;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public T? Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T? Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T? next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T? last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }


    [Serializable]
    public class LinkedGraph<T> : ILinkedGraph<T>, ILinkedGraphHasLink<T>, ILinkedGraphLink<T>, ILinkedGraphUnLink<T>, ILinkedGraphTryGet<T>, ILinkedGraphTryGetLinkValue<T>, ILinkedGraphNextLast<T>, ILinkedGraphTryNextLast<T>, IDataGraph<T>, IDataGraphHas<T>, IDataGraphSet<T>, IDataGraphTryGet<T>, IDataGraphUnSet<T>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : class
    {
        protected class Node
        {
            public ConcurrentDictionary<T, ConcurrentDictionary<Type, object>> bind = new ConcurrentDictionary<T, ConcurrentDictionary<Type, object>>();
            public T Last { get; set; }
            public T Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>> link_table = new ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<object> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.ContainsKey(typeof(V)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            return val.Equals(value);
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public void Set<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                var val_set = from_node.bind.GetOrAdd(to, i => new ConcurrentDictionary<Type, object>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.AddOrUpdate(typeof(V), value, (_t, _v) => value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        return values.TryRemove(typeof(V), out var _);
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                return values.TryRemove(typeof(V), out var _);
                            }
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<object> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals.Values;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    link_table.TryRemove((item, node.Next), out var _1);
                    link_table.TryRemove((node.Last, item), out var _2);
                    if (node.Next != null && inner_table.TryGetValue(node.Next, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    return vals.ContainsKey(typeof(V));
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2, V value) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        return val.Equals(value);
                    }
                }
                return false;
            }
        }

        virtual public void Link<V>(T item1, T item2, V value) where V : class
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                var vals = link_table.GetOrAdd((item1, item2), (_) => new ConcurrentDictionary<Type, object>());
                link_table.GetOrAdd((item2, item1), (_) => vals);
                vals.AddOrUpdate(typeof(V), value, (_t, _v) => value);
                from_node.Next = item2;
                to_node.Last = item1;
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue<V>(T item1, T item2, out V value) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if(vals.TryGetValue(typeof(V), out var val))
                    {
                        value = (V)val;
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }

        virtual public T Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }

    [Serializable]
    public class LinkedGraphValue<T> : ILinkedGraph<T>, ILinkedGraphHasLink<T>, ILinkedGraphValueLink<T>, ILinkedGraphUnLink<T>, ILinkedGraphTryGet<T>, ILinkedGraphValueTryGetLinkValue<T>, ILinkedGraphNextLast<T>, ILinkedGraphTryNextLast<T>, IDataGraph<T>, IDataGraphValueHas<T>, IDataGraphValueSet<T>, IDataGraphTryGet<T>, IDataGraphValueUnSet<T>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : class
    {
        protected class Node
        {
            public ConcurrentDictionary<T, ConcurrentDictionary<Type, object>> bind = new ConcurrentDictionary<T, ConcurrentDictionary<Type, object>>();
            public T Last { get; set; }
            public T Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>> link_table = new ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<object> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.ContainsKey(typeof(V)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            return val.Equals(value);
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public void Set<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                var val_set = from_node.bind.GetOrAdd(to, i => new ConcurrentDictionary<Type, object>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.AddOrUpdate(typeof(V), value, (_t, _v) => value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        return values.TryRemove(typeof(V), out var _);
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                return values.TryRemove(typeof(V), out var _);
                            }
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<object> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals.Values;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    link_table.TryRemove((item, node.Next), out var _1);
                    link_table.TryRemove((node.Last, item), out var _2);
                    if (node.Next != null && inner_table.TryGetValue(node.Next, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    return vals.ContainsKey(typeof(V));
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2, V value) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        return val.Equals(value);
                    }
                }
                return false;
            }
        }

        virtual public void Link<V>(T item1, T item2, V value) where V : struct
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                var vals = link_table.GetOrAdd((item1, item2), (_) => new ConcurrentDictionary<Type, object>());
                link_table.GetOrAdd((item2, item1), (_) => vals);
                vals.AddOrUpdate(typeof(V), value, (_t, _v) => value);
                from_node.Next = item2;
                to_node.Last = item1;
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue<V>(T item1, T item2, out V? value) where V : struct
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        value = (V)val;
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }

        virtual public T Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }

    [Serializable]
    public class ValueLinkedGraph<T> : ILinkedGraph<T>, ILinkedGraphHasLink<T>, ILinkedGraphLink<T>, ILinkedGraphUnLink<T>, ILinkedGraphTryGet<T>, ILinkedGraphTryGetLinkValue<T>, IValueLinkedGraphNextLast<T>, IValueLinkedGraphTryNextLast<T>, IDataGraph<T>, IDataGraphHas<T>, IDataGraphSet<T>, IDataGraphTryGet<T>, IDataGraphUnSet<T>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : struct
    {
        protected class Node
        {
            public ConcurrentDictionary<T, ConcurrentDictionary<Type, object>> bind = new ConcurrentDictionary<T, ConcurrentDictionary<Type, object>>();
            public T? Last { get; set; }
            public T? Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>> link_table = new ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<object> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.ContainsKey(typeof(V)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            return val.Equals(value);
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public void Set<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                var val_set = from_node.bind.GetOrAdd(to, i => new ConcurrentDictionary<Type, object>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.AddOrUpdate(typeof(V), value, (_t, _v) => value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        return values.TryRemove(typeof(V), out var _);
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                return values.TryRemove(typeof(V), out var _);
                            }
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<object> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals.Values;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    if(node.Next.HasValue) link_table.TryRemove((item, node.Next.Value), out var _1);
                    if(node.Last.HasValue) link_table.TryRemove((node.Last.Value, item), out var _2);
                    if (node.Next != null && inner_table.TryGetValue(node.Next.Value, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last.Value, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    return vals.ContainsKey(typeof(V));
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2, V value) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        return val.Equals(value);
                    }
                }
                return false;
            }
        }

        virtual public void Link<V>(T item1, T item2, V value) where V : class
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                var vals = link_table.GetOrAdd((item1, item2), (_) => new ConcurrentDictionary<Type, object>());
                link_table.GetOrAdd((item2, item1), (_) => vals);
                vals.AddOrUpdate(typeof(V), value, (_t, _v) => value);
                from_node.Next = item2;
                to_node.Last = item1;
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue<V>(T item1, T item2, out V value) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        value = (V)val;
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }

        virtual public T? Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T? Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T? next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T? last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }

    [Serializable]
    public class ValueLinkedGraphValue<T> : ILinkedGraph<T>, ILinkedGraphHasLink<T>, ILinkedGraphValueLink<T>, ILinkedGraphUnLink<T>, ILinkedGraphTryGet<T>, ILinkedGraphValueTryGetLinkValue<T>, IValueLinkedGraphNextLast<T>, IValueLinkedGraphTryNextLast<T>, IDataGraph<T>, IDataGraphValueHas<T>, IDataGraphValueSet<T>, IDataGraphTryGet<T>, IDataGraphValueUnSet<T>, IGraph<T>, IGraphHas<T>, IGraphUnSet<T>
        where T : struct
    {
        protected class Node
        {
            public ConcurrentDictionary<T, ConcurrentDictionary<Type, object>> bind = new ConcurrentDictionary<T, ConcurrentDictionary<Type, object>>();
            public T? Last { get; set; }
            public T? Next { get; set; }
        }

        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();
        protected ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>> link_table = new ConcurrentDictionary<(T, T), ConcurrentDictionary<Type, object>>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<object> this[T from, T to] => TryGetValues(from, to, out var vals) ? vals : null;
        virtual public IEnumerable<T> this[T index] => TryGetBinds(index, out var vals) ? vals : null;

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
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.ContainsKey(to) && to_node.bind.ContainsKey(from))
                    {
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.ContainsKey(typeof(V)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool Has<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            return val.Equals(value);
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public void Set<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                var val_set = from_node.bind.GetOrAdd(to, i => new ConcurrentDictionary<Type, object>());
                to_node.bind.AddOrUpdate(from, val_set, (_t, _v) => val_set);
                val_set.AddOrUpdate(typeof(V), value, (_t, _v) => value);
            }
        }

        virtual public bool UnSet(T from, T to)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    var f = from_node.bind.TryRemove(to, out var _);
                    var t = to_node.bind.TryRemove(from, out var _);
                    if (f || t)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        return values.TryRemove(typeof(V), out var _);
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var values) && to_node.bind.ContainsKey(from))
                    {
                        if (values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                return values.TryRemove(typeof(V), out var _);
                            }
                        }
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
                    }
                }
                return false;
            }
        }

        virtual public bool TryGetBinds(T from, out IEnumerable<T> binds)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    binds = from_node.bind.Keys;
                    return true;
                }
            }
            binds = null;
            return false;
        }
        virtual public bool TryGetValues(T from, T to, out IEnumerable<object> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && inner_table.TryGetValue(to, out var to_node))
                {
                    if (from_node.bind.TryGetValue(to, out var vals) && to_node.bind.ContainsKey(from))
                    {
                        values = vals.Values;
                        return true;
                    }
                    else
                    {
                        from_node.bind.TryRemove(to, out var _);
                        to_node.bind.TryRemove(from, out var _);
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
                    foreach (var bind in node.bind.Keys)
                    {
                        if (inner_table.TryGetValue(bind, out var bind_node))
                        {
                            bind_node.bind.TryRemove(item, out var _);
                        }
                    }
                    if (node.Next.HasValue) link_table.TryRemove((item, node.Next.Value), out var _1);
                    if (node.Last.HasValue) link_table.TryRemove((node.Last.Value, item), out var _2);
                    if (node.Next != null && inner_table.TryGetValue(node.Next.Value, out var next))
                    {
                        next.Last = null;
                    }
                    if (node.Last != null && inner_table.TryGetValue(node.Last.Value, out var last))
                    {
                        last.Next = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool HasLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.ContainsKey((item1, item2)))
                {
                    return true;
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    return vals.ContainsKey(typeof(V));
                }
                return false;
            }
        }
        virtual public bool HasLink<V>(T item1, T item2, V value) where V : class
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        return val.Equals(value);
                    }
                }
                return false;
            }
        }

        virtual public void Link<V>(T item1, T item2, V value) where V : struct
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(item1);
                var to_node = GetOrAddNode(item2);
                var vals = link_table.GetOrAdd((item1, item2), (_) => new ConcurrentDictionary<Type, object>());
                link_table.GetOrAdd((item2, item1), (_) => vals);
                vals.AddOrUpdate(typeof(V), value, (_t, _v) => value);
                from_node.Next = item2;
                to_node.Last = item1;
            }
        }

        virtual public bool UnLink(T item1, T item2)
        {
            lock (WriteLock)
            {
                if (link_table.TryRemove((item1, item2), out var _))
                {
                    if (inner_table.TryGetValue(item1, out var a_node))
                    {
                        a_node.Next = null;
                    }
                    if (inner_table.TryGetValue(item2, out var b_node))
                    {
                        b_node.Last = null;
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public bool TryGetLinkValue<V>(T item1, T item2, out V? value) where V : struct
        {
            lock (WriteLock)
            {
                if (link_table.TryGetValue((item1, item2), out var vals))
                {
                    if (vals.TryGetValue(typeof(V), out var val))
                    {
                        value = (V)val;
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }

        virtual public T? Next(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Next;
                }
                return null;
            }
        }

        virtual public T? Last(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    return node.Last;
                }
                return null;
            }
        }

        virtual public bool TryNext(T item, out T? next)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    next = node.Next;
                    if (next != null) return true;
                }
                next = null;
                return false;
            }
        }

        virtual public bool TryLast(T item, out T? last)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(item, out var node))
                {
                    last = node.Last;
                    if (last != null) return true;
                }
                last = null;
                return false;
            }
        }
    }
}
