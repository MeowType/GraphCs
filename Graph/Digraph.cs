using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    [Serializable]
    public class Digraph<T> : IDigraph<T>, IDigraphGet<T, IEnumerable<T>>, IReadOnlyCollection<T>
    {
        [Serializable]
        protected class Box
        {
            public HashSet<T> to = new HashSet<T>();
            public HashSet<T> from = new HashSet<T>();
        }

        protected ConcurrentDictionary<T, Box> inner_table = new ConcurrentDictionary<T, Box>();

        virtual public int Count => inner_table.Count;

        virtual public bool IsReadOnly => false;

        virtual public IEnumerable<T> this[T index] => TryGetTo(index, out var vals) ? vals : null;

        [NonSerialized]
        protected object WriteLock = new object();

        virtual public void Add(T item)
        {
            lock (WriteLock)
            {
                inner_table.GetOrAdd(item, _ => new Box());
            }
        }

        virtual protected Box GetOrAddBox(T item)
        {
            return inner_table.GetOrAdd(item, _ => new Box());
        }

        virtual public void Clear()
        {
            inner_table.Clear();
        }

        virtual public bool Contains(T item)
        {
            return Has(item);
        }

        virtual public void CopyTo(T[] array, int arrayIndex)
        {
            inner_table.Keys.CopyTo(array, arrayIndex);
        }

        virtual public IEnumerator<T> GetEnumerator()
        {
            return inner_table.Keys.GetEnumerator();
        }

        virtual public bool Has(T key)
        {
            return inner_table.ContainsKey(key);
        }

        virtual public bool Has(T key, T value)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(key, out var box))
                {
                    if (box.to.Contains(value))
                    {

                        if (inner_table.ContainsKey(value))
                        {
                            return true;
                        }
                        else
                        {
                            box.to.Remove(value);
                        }
                    }
                }
                return false;
            }
        }

        virtual public bool Remove(T key)
        {
            lock (WriteLock)
            {
                if (inner_table.TryRemove(key, out var box))
                {
                    foreach (var from in box.from)
                    {
                        if (inner_table.TryGetValue(from, out var from_box))
                        {
                            from_box.to.Remove(key);
                        }
                    }
                    foreach (var to in box.to)
                    {
                        if (inner_table.TryGetValue(to, out var to_box))
                        {
                            to_box.from.Remove(key);
                        }
                    }
                    return true;
                }
                return false;
            }
        }

        virtual public void Set(T a, T b)
        {
            lock (WriteLock)
            {
                var box = GetOrAddBox(a);
                var b_box = GetOrAddBox(b);
                box.to.Add(b);
                b_box.from.Add(a);
            }
        }

        virtual public bool UnSet(T a, T b)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(a, out var box))
                {
                    if (inner_table.TryGetValue(b, out var b_box))
                    {
                        box.to.Remove(b);
                        b_box.from.Remove(a);
                        return true;
                    }
                }
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner_table.Keys.GetEnumerator();
        }

        virtual public bool TryGetTo(T key, out IEnumerable<T> values)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(key, out var box))
                {
                    values = box.to;
                    return true;
                }
            }
            values = null;
            return false;
        }

        virtual public bool TryGetFrom(T value, out IEnumerable<T> keys)
        {
            lock (WriteLock)
            {
                if (inner_table.TryGetValue(value, out var box))
                {
                    keys = box.from;
                    return true;
                }
            }
            keys = null;
            return false;
        }
    }

    [Serializable]
    public class MutualGraph<T> : Digraph<T>
    {
        public override void Set(T a, T b)
        {
            lock (WriteLock)
            {
                base.Set(a, b);
                base.Set(b, a);
            }
        }

        public override bool UnSet(T a, T b)
        {
            lock (WriteLock)
            {
                var ba = base.UnSet(a, b);
                var bb = base.UnSet(b, a);
                return ba || bb;
            }
        }
    }
}
