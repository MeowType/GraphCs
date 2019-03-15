using System.Collections;
using System.Collections.Generic;

namespace MeowType.Graph
{
    public interface IGraphGet<T, E> where E : IEnumerable<T>
    {
        E this[T index] { get; }
        bool TryGetTo(T key, out E values);
    }

    public interface IDigraphGet<T, E> : IGraphGet<T, E> where E : IEnumerable<T>
    {
        bool TryGetFrom(T value, out E keys);
    }

    public interface IGraph<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        bool Has(T key);
        bool Has(T key, T value);
    }

    public interface IDigraph<T> : IGraph<T>
    {
        void Set(T a, T b);
        bool UnSet(T a, T b);
    }
}
