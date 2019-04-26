using System.Collections.Generic;

namespace MeowType.Collections.Graph
{
    #region <T, V>

    public interface IDataGraphSet<T, V>: IDataGraph<T, V>
    {
        void Set(T item1, T item2, V value);
    }

    public interface IDataGraphHas<T, V>: IReadOnlyDataGraph<T, V>
    {
        bool Has(T item1, T item2, V value);
    }

    public interface IDataGraphUnSet<T, V>: IDataGraph<T, V>
    {
        bool UnSet(T item1, T item2);
        bool UnSet(T item1, T item2, V value);
    }

    public interface IDataGraph<T, V> : IGraph<T> , IReadOnlyDataGraph<T, V> { }
    public interface IReadOnlyDataGraph<T, V> : IReadOnlyGraph<T> { }

    public interface IDataGraphTryGet<T, V>
    {
        IEnumerable<V> this[T from, T to] { get; }
        bool TryGetValues(T from, T to, out IEnumerable<V> values);
    }

    #endregion

    #region <T>

    public interface IDataGraphSet<T> : IDataGraph<T>
    {
        void Set<V>(T item1, T item2, V value) where V : class;
    }

    public interface IDataGraphHas<T> : IReadOnlyDataGraph<T>, IGraphHas<T>
    {
        bool Has<V>(T item1, T item2) where V : class;
        bool Has<V>(T item1, T item2, V value) where V : class;
    }

    public interface IDataGraphUnSet<T> : IDataGraph<T> , IGraphUnSet<T>
    {
        bool UnSet<V>(T item1, T item2) where V : class;
        bool UnSet<V>(T item1, T item2, V value) where V : class;
    }

    public interface IDataGraphTryGet<T> : IReadOnlyDataGraph<T>
    {
        bool TryGetValues(T from, T to, out IEnumerable<object> values);
    }

    public interface IDataGraphValueSet<T> : IDataGraph<T>
    {
        void Set<V>(T item1, T item2, V value) where V : struct;
    }

    public interface IDataGraphValueHas<T> : IReadOnlyDataGraph<T>, IGraphHas<T>
    {
        bool Has<V>(T item1, T item2) where V : struct;
        bool Has<V>(T item1, T item2, V value) where V : struct;
    }

    public interface IDataGraphValueUnSet<T> : IDataGraph<T>, IGraphUnSet<T>
    {
        bool UnSet<V>(T item1, T item2) where V : struct;
        bool UnSet<V>(T item1, T item2, V value) where V : struct;
    }

    public interface IDataGraph<T> : IGraph<T>, IReadOnlyDataGraph<T> { }
    public interface IReadOnlyDataGraph<T> : IReadOnlyGraph<T> { }

    #endregion

    #region SingleGraph

    public interface IDataSingleGraphGet<T, V> : IReadOnlyDataGraph<T, V> where V : class 
    {
        bool TryGetValue(T from, T to, out V value);
    }

    public interface IDataSingleGraphValueGet<T, V> : IReadOnlyDataGraph<T, V> where V : struct
    {
        bool TryGetValue(T from, T to, out V? value);
    }

    public interface IDataSingleGraphGet<T> : IReadOnlyDataSingleGraph<T>
    {
        bool TryGetValue<V>(T from, T to, out V value) where V : class;
    }

    public interface IDataSingleGraphValueGet<T> : IReadOnlyDataSingleGraph<T>
    {
        bool TryGetValue<V>(T from, T to, out V? value) where V : struct;
    }

    public interface IDataSingleGraph<T, V> : IDataGraph<T, V>, IReadOnlyDataSingleGraph<T, V> { }
    public interface IReadOnlyDataSingleGraph<T, V> : IReadOnlyDataGraph<T, V> { }
    public interface IDataSingleGraph<T> : IDataGraph<T>, IReadOnlyDataSingleGraph<T> { }
    public interface IReadOnlyDataSingleGraph<T> : IReadOnlyDataGraph<T> { }

    #endregion
}
