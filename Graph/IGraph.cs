using System.Collections;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    public interface IGraphGet<T, E> where E : IEnumerable<T>
    {
        E this[T index] { get; }
        bool TryGetTo(T item, out E connections);
    }

    public interface IDigraphGet<T, E> where E : IEnumerable<T>
    {
        bool TryGetFrom(T to, out E froms);
    }

    public interface IValueGraphGet<T, V, E> where E : IEnumerable<V>
    {
        E this[T from, T to] { get; }
        bool TryGetValues(T from, T to, out E values);
    }

    public interface IValueGraphSet<T, V>
    {
        void Set(T item1, T item2, V value);
    }

    public interface IValueGraphSetIndex<T, V> : IValueGraphSet<T, V>
    {
        V this[T from, T to] { set; }
    }

    public interface IReadOnlyGraph<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
    {
        bool Has(T item);
        bool Has(T item1, T item2);
    }

    public interface IGraph<T> : IReadOnlyGraph<T>
    {
        void Set(T item1, T item2);
        bool UnSet(T item1, T item2);
    }

    public interface IValueGraph<T, V> : IReadOnlyGraph<T>, IValueGraphSet<T, V>
    {
        bool Has(T item1, T item2, V value);
        bool UnSet(T item1, T item2);
        bool UnSet(T item1, T item2, V value);
    }

}
