using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    [Serializable]
    public class ValueSingleGraphValue<T, V> : IGraph<T> , IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, IValueSingleDigraphTryGet<T>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphUnSet<T, V>, IDataSingleGraph<T, V>, IDataSingleGraphValueGet<T, V>
        where T : struct where V : struct
    {
        [Serializable]
        protected class Node
        {
            public T? to = null;
            public T? from = null;
            public V? value = null;
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public V? this[T from, T to] => TryGetValue(from, to, out var val) ? val : null;
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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.value.Equals(value))
                            {
                                return true;
                            }
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

        virtual public void Set(T from, T to, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.value = value;
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.value.Equals(value))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
        virtual public bool TryGetValue(T from, T to, out V? value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.value != null)
                {
                    value = from_node.value;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public bool Remove(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(item, out var node))
                {
                    if(node.from.HasValue)
                    {
                        if (inner_table.TryGetValue(node.from.Value, out var from_node))
                        {
                            from_node.to = null;
                            from_node.value = null;
                        }
                    }
                    if (node.to.HasValue)
                    {
                        if (inner_table.TryGetValue(node.to.Value, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.value = null;
                    return true;
                }
                return false;
            }
        }
    }
    [Serializable]
    public class SingleGraphValue<T, V> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, IValueSingleDigraphTryGet<T>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphUnSet<T, V>, IDataSingleGraph<T, V>, IDataSingleGraphGet<T, V>
        where T : struct where V : class
    {
        [Serializable]
        protected class Node
        {
            public T? to = null;
            public T? from = null;
            public V value = null;
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public V this[T from, T to] => TryGetValue(from, to, out var val) ? val : null;
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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.value.Equals(value))
                            {
                                return true;
                            }
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

        virtual public void Set(T from, T to, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.value = value;
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.value.Equals(value))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
        virtual public bool TryGetValue(T from, T to, out V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.value != null)
                {
                    value = from_node.value;
                    return true;
                }
                value = null;
                return false;
            }
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
                            from_node.value = null;
                        }
                    }
                    if (node.to.HasValue)
                    {
                        if (inner_table.TryGetValue(node.to.Value, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.value = null;
                    return true;
                }
                return false;
            }
        }
    }

    [Serializable]
    public class ValueSingleGraph<T, V> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, ISingleDigraphTryGet<T>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphUnSet<T, V>, IDataSingleGraph<T, V>, IDataSingleGraphValueGet<T, V>
        where T : class where V : struct
    {
        [Serializable]
        protected class Node
        {
            public T to = null;
            public T from = null;
            public V? value = null;
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public V? this[T from, T to] => TryGetValue(from, to, out var val) ? val : null;
        virtual public T this[T index] => TryGetTo(index, out var val) ? val : null;

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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.value.Equals(value))
                            {
                                return true;
                            }
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

        virtual public void Set(T from, T to, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.value = value;
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.value.Equals(value))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
        virtual public bool TryGetValue(T from, T to, out V? value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.value != null)
                {
                    value = from_node.value;
                    return true;
                }
                value = null;
                return false;
            }
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
                            from_node.value = null;
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.value = null;
                    return true;
                }
                return false;
            }
        }
    }
    [Serializable]
    public class SingleGraph<T, V> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, ISingleDigraphTryGet<T>, IDataGraph<T, V>, IDataGraphHas<T, V>, IDataGraphSet<T, V>, IDataGraphUnSet<T, V>, IDataSingleGraph<T, V>, IDataSingleGraphGet<T, V>
        where T : class where V : class
    {
        [Serializable]
        protected class Node
        {
            public T to = null;
            public T from = null;
            public V value = null;
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public V this[T from, T to] => TryGetValue(from, to, out var val) ? val : null;
        virtual public T this[T index] => TryGetTo(index, out var val) ? val : null;

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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has(T from, T to, V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.value.Equals(value))
                            {
                                return true;
                            }
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

        virtual public void Set(T from, T to, V value)
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.value = value;
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.value.Equals(value))
                        {
                            from_node.to = null;
                            from_node.value = null;
                            to_node.from = null;
                        }
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
        virtual public bool TryGetValue(T from, T to, out V value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.value != null)
                {
                    value = from_node.value;
                    return true;
                }
                value = null;
                return false;
            }
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
                            from_node.value = null;
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.value = null;
                    return true;
                }
                return false;
            }
        }
    }

    [Serializable]
    public class ValueSingleGraphValue<T> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, IValueSingleDigraphTryGet<T>, IDataGraph<T>, IDataGraphValueHas<T>, IDataGraphValueSet<T>, IDataGraphValueUnSet<T>, IDataSingleGraph<T>, IDataSingleGraphValueGet<T>
        where T : struct
    {
        [Serializable]
        protected class Node
        {
            public T? to = null;
            public T? from = null;
            public ConcurrentDictionary<Type, object> values = new ConcurrentDictionary<Type, object>();
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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has<V>(T from, T to) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.ContainsKey(typeof(V)))
                            {
                                return true;
                            }
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
        virtual public bool Has<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.TryGetValue(typeof(V), out var val))
                            {
                                return val.Equals(value);
                            }
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

        virtual public void Set<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.values.AddOrUpdate(typeof(V), value, (_t, _v) => value);
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.values.Clear();
                            to_node.from = null;
                        }
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
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.ContainsKey(typeof(V)))
                        {
                            from_node.values.TryRemove(typeof(V), out var _);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                from_node.values.TryRemove(typeof(V), out var _);
                                return true;
                            }
                        }
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
        virtual public bool TryGetValue<V>(T from, T to, out V? value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                {
                    value = (V)val;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public bool Remove(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(item, out var node))
                {
                    if (node.from != null)
                    {
                        if (inner_table.TryGetValue(node.from.Value, out var from_node))
                        {
                            from_node.to = null;
                            from_node.values.Clear();
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to.Value, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.values.Clear();
                    return true;
                }
                return false;
            }
        }
    }
    [Serializable]
    public class SingleGraphValue<T> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, IValueSingleDigraphTryGet<T>, IDataGraph<T>, IDataGraphHas<T>, IDataGraphSet<T>, IDataGraphUnSet<T>, IDataSingleGraph<T>, IDataSingleGraphGet<T> 
        where T : struct
    {
        [Serializable]
        protected class Node
        {
            public T? to = null;
            public T? from = null;
            public ConcurrentDictionary<Type, object> values = new ConcurrentDictionary<Type, object>();
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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has<V>(T from, T to) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.ContainsKey(typeof(V)))
                            {
                                return true;
                            }
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
        virtual public bool Has<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.TryGetValue(typeof(V), out var val))
                            {
                                return val.Equals(value);
                            }
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

        virtual public void Set<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.values.AddOrUpdate(typeof(V), value, (_t, _v) => value);
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.values.Clear();
                            to_node.from = null;
                        }
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
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.ContainsKey(typeof(V)))
                        {
                            from_node.values.TryRemove(typeof(V), out var _);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                from_node.values.TryRemove(typeof(V), out var _);
                                return true;
                            }
                        }
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
        virtual public bool TryGetValue<V>(T from, T to, out V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                {
                    value = (V)val;
                    return true;
                }
                value = null;
                return false;
            }
        }

        virtual public bool Remove(T item)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(item, out var node))
                {
                    if (node.from != null)
                    {
                        if (inner_table.TryGetValue(node.from.Value, out var from_node))
                        {
                            from_node.to = null;
                            from_node.values.Clear();
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to.Value, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.values.Clear();
                    return true;
                }
                return false;
            }
        }
    }

    [Serializable]
    public class ValueSingleGraph<T> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, ISingleDigraphTryGet<T>, IDataGraph<T>, IDataGraphValueHas<T>, IDataGraphValueSet<T>, IDataGraphValueUnSet<T>, IDataSingleGraph<T>, IDataSingleGraphValueGet<T>
        where T : class
    {
        [Serializable]
        protected class Node
        {
            public T to = null;
            public T from = null;
            public ConcurrentDictionary<Type, object> values = new ConcurrentDictionary<Type, object>();
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public T this[T index] => TryGetTo(index, out var val) ? val : null;

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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has<V>(T from, T to) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.ContainsKey(typeof(V)))
                            {
                                return true;
                            }
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
        virtual public bool Has<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.TryGetValue(typeof(V), out var val))
                            {
                                return val.Equals(value);
                            }
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

        virtual public void Set<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.values.AddOrUpdate(typeof(V), value, (_t, _v) => value);
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.values.Clear();
                            to_node.from = null;
                        }
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
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.ContainsKey(typeof(V)))
                        {
                            from_node.values.TryRemove(typeof(V), out var _);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                from_node.values.TryRemove(typeof(V), out var _);
                                return true;
                            }
                        }
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
        virtual public bool TryGetValue<V>(T from, T to, out V? value) where V : struct
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                {
                    value = (V)val;
                    return true;
                }
                value = null;
                return false;
            }
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
                            from_node.values.Clear();
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.values.Clear();
                    return true;
                }
                return false;
            }
        }
    }
    [Serializable]
    public class SingleGraph<T> : IGraph<T>, IGraphHas<T>, IGraphUnSet<T>, ISingleDigraph<T>, ISingleDigraphTryGet<T>, IDataGraph<T>, IDataGraphHas<T>, IDataGraphSet<T>, IDataGraphUnSet<T>, IDataSingleGraph<T>, IDataSingleGraphGet<T>
        where T : class 
    {
        [Serializable]
        protected class Node
        {
            public T to = null;
            public T from = null;
            public ConcurrentDictionary<Type, object> values = new ConcurrentDictionary<Type, object>();
        }
        protected ConcurrentDictionary<T, Node> inner_table = new ConcurrentDictionary<T, Node>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public T this[T index] => TryGetTo(index, out var val) ? val : null;

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
                    if (from_node.to.Equals(to))
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
        virtual public bool Has<V>(T from, T to) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.ContainsKey(typeof(V)))
                            {
                                return true;
                            }
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
        virtual public bool Has<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (from_node.to.Equals(to))
                    {
                        if (inner_table.ContainsKey(to))
                        {
                            if (from_node.values.TryGetValue(typeof(V), out var val))
                            {
                                return val.Equals(value);
                            }
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

        virtual public void Set<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                var from_node = GetOrAddNode(from);
                var to_node = GetOrAddNode(to);
                from_node.to = to;
                from_node.values.AddOrUpdate(typeof(V), value, (_t, _v) => value);
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
                        if (from_node.to.Equals(to))
                        {
                            from_node.to = null;
                            from_node.values.Clear();
                            to_node.from = null;
                        }
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
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.ContainsKey(typeof(V)))
                        {
                            from_node.values.TryRemove(typeof(V), out var _);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        virtual public bool UnSet<V>(T from, T to, V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node))
                {
                    if (inner_table.TryGetValue(to, out var to_node))
                    {
                        if (from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                        {
                            if (val.Equals(value))
                            {
                                from_node.values.TryRemove(typeof(V), out var _);
                                return true;
                            }
                        }
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
        virtual public bool TryGetValue<V>(T from, T to, out V value) where V : class
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(from, out var from_node) && from_node.to.Equals(to) && from_node.values.TryGetValue(typeof(V), out var val))
                {
                    value = (V)val;
                    return true;
                }
                value = null;
                return false;
            }
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
                            from_node.values.Clear();
                        }
                    }
                    if (node.to != null)
                    {
                        if (inner_table.TryGetValue(node.to, out var to_node))
                        {
                            to_node.from = null;
                        }
                    }
                    node.values.Clear();
                    return true;
                }
                return false;
            }
        }
    }
}
