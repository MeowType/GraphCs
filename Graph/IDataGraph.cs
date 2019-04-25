using System;
using System.Collections.Generic;
using System.Text;

namespace MeowType.Collections.Graph
{
    #region <T, V>

    public interface IDataGraphSet<T, V>
    {
        void Set(T item1, T item2, V value);
    }

    public interface IDataGraphSetIndex<T, V> : IDataGraphSet<T, V>
    {
        V this[T from, T to] { set; }
    }

    public interface IDataGraph<T, V> : IReadOnlyGraph<T>, IDataGraphSet<T, V>
    {
        bool Has(T item1, T item2, V value);
        bool UnSet(T item1, T item2);
        bool UnSet(T item1, T item2, V value);
    }

    public interface IDataGraphTryGet<T, V, E> where E : IEnumerable<V>
    {
        E this[T from, T to] { get; }
        bool TryGetValues(T from, T to, out E values);
    }

    #endregion

    #region <T>

    public interface IDataGraphSet<T>
    {
        void Set<V>(T item1, T item2, V value) where V : class;
    }

    public interface IDataGraph<T> : IReadOnlyGraph<T>, IDataGraphSet<T>
    {
        bool Has<V>(T item1, T item2, V value) where V : class;
        bool UnSet(T item1, T item2);
        bool UnSet<V>(T item1, T item2, V value) where V : class;
    }

    public interface IDataGraphTryGet<T>
    {
        bool TryGetValues<V>(T from, T to, out IEnumerable<V> values) where V : class;
    }

    public interface IDataGraphValueSet<T>
    {
        void Set<V>(T item1, T item2, V value) where V : struct;
    }

    public interface IDataGraphValue<T> : IReadOnlyGraph<T>, IDataGraphValueSet<T>
    {
        bool Has<V>(T item1, T item2) where V : struct;
        bool Has<V>(T item1, T item2, V value) where V : struct;
        bool UnSet(T item1, T item2);
        bool UnSet<V>(T item1, T item2) where V : struct;
        bool UnSet<V>(T item1, T item2, V value) where V : struct;
    }

    #endregion

    #region SingleGraph

    public interface IDataSingleGraphGet<T, V>
    {
        V this[T from, T to] { get; }
        bool TryGetValue(T from, T to, out V value);
    }

    public interface IDataValueSingleGraphGet<T, V> where V : struct
    {
        V? this[T from, T to] { get; }
        bool TryGetValue(T from, T to, out V? value);
    }

    public interface IDataSingleGraphGet<T>
    {
        bool TryGetValue<V>(T from, T to, out V value) where V : class;
    }

    public interface IDataValueSingleGraphGet<T>
    {
        bool TryGetValue<V>(T from, T to, out V? value) where V : struct;
    }

    #endregion
}
