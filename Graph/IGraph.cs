using System.Collections;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    public interface IGraphTryGet<T, E> where E : IEnumerable<T>
    {
        E this[T index] { get; }
        bool TryGetTo(T item, out E connections);
    }

    public interface IDigraphTryGet<T, E> where E : IEnumerable<T>
    {
        bool TryGetFrom(T to, out E froms);
    }

    public interface ISingleGraphTryGet<T>
    {
        T this[T index] { get; }
        bool TryGetTo(T item, out T connection);
        bool TryGetFrom(T to, out T from);
    }

    public interface ISingleGraphValueTryGet<T> where T : struct
    {
        T? this[T index] { get; }
        bool TryGetTo(T item, out T? connection);
        bool TryGetFrom(T to, out T? from);
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
    
}
