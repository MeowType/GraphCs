using System.Collections;
using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    public interface IDigraphTryGet<T> : IReadOnlyDiGraph<T>
    {
        bool TryGetTo(T item, out IEnumerable<T> connections);
        bool TryGetFrom(T to, out IEnumerable<T> froms);
    }

    public interface ISingleDigraphTryGet<T> : IReadOnlySingleDigraph<T>
    {
        bool TryGetTo(T item, out T connection);
        bool TryGetFrom(T to, out T from);
    }

    public interface IValueSingleDigraphTryGet<T> : IReadOnlySingleDigraph<T> where T : struct
    {
        bool TryGetTo(T item, out T? connection);
        bool TryGetFrom(T to, out T? from);
    }

    public interface IGraphSet<T> : IGraph<T>
    {
        void Set(T item1, T item2);
    }

    public interface IGraphUnSet<T> : IGraph<T>
    {
        bool UnSet(T item1, T item2);
    }

    public interface IGraphHas<T> : IReadOnlyGraph<T>
    {
        bool Has(T item);
        bool Has(T item1, T item2);
    }


    public interface ISingleDigraph<T> : IGraph<T>, IReadOnlySingleDigraph<T> { }
    public interface IReadOnlySingleDigraph<T> : IReadOnlyGraph<T> { }

    public interface IDigraph<T> : IGraph<T>, IReadOnlyDiGraph<T> { }
    public interface IReadOnlyDiGraph<T> : IReadOnlyGraph<T> { }

    public interface IGraph<T> : IReadOnlyGraph<T>, ICollection<T> { }
    public interface IReadOnlyGraph<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T> { }
}
