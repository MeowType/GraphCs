using System;
using System.Collections.Generic;
using System.Text;

namespace MeowType.Collections.Graph
{
    #region <T, V>

    public interface ILinkedGraphTryGet<T, V> : IReadOnlyLinkedGraph<T, V>
    {
        bool TryGetBinds(T item, out IEnumerable<T> binds);
    }
    public interface ILinkedGraphHasLink<T, V> : IReadOnlyLinkedGraph<T, V>
    { 
        bool HasLink(T item1, T item2);
        bool HasLink(T item1, T item2, V value);
    }

    public interface ILinkedGraphUnLink<T, V> : ILinkedGraph<T, V>
    {
        bool UnLink(T item1, T item2);
    }

    public interface ILinkedGraphLink<T, V> : ILinkedGraph<T, V>
    {
        void Link(T item1, T item2, V value);
    }

    public interface ILinkedGraphTryGetLinkValue<T, V> : IReadOnlyLinkedGraph<T, V> where V : class
    {
        bool TryGetLinkValue(T item1, T item2, out V value);
    }

    public interface ILinkedGraphNextLast<T, V> : IReadOnlyLinkedGraph<T, V> where T : class
    {
        T Next(T item);
        T Last(T item);
    }

    public interface ILinkedGraphTryNextLast<T, V> : IReadOnlyLinkedGraph<T, V> where T : class
    {
        bool TryNext(T item, out T next);
        bool TryLast(T item, out T last);
    }

    public interface ILinkedGraphValueTryGetLinkValue<T, V> : IReadOnlyLinkedGraph<T, V> where V : struct
    {
        bool TryGetLinkValue(T item1, T item2, out V? value);
    }

    public interface IValueLinkedGraphNextLast<T, V> : IReadOnlyLinkedGraph<T, V> where T : struct
    {
        T? Next(T item);
        T? Last(T item);
    }

    public interface IValueLinkedGraphTryNextLast<T, V> : IReadOnlyLinkedGraph<T, V> where T : struct
    {
        bool TryNext(T item, out T? next);
        bool TryLast(T item, out T? last);
    }

    public interface ILinkedGraph<T, V> : IDataGraph<T, V>, IReadOnlyLinkedGraph<T, V> { }
    public interface IReadOnlyLinkedGraph<T, V> : IDataGraph<T, V> { }

    #endregion

    #region <T>

    public interface ILinkedGraphTryGet<T> : IReadOnlyLinkedGraph<T>
    {
        bool TryGetBinds(T item, out IEnumerable<T> binds);
    }
    public interface ILinkedGraphHasLink<T> : IReadOnlyLinkedGraph<T>
    {
        bool HasLink(T item1, T item2) ;
        bool HasLink<V>(T item1, T item2) where V : class;
        bool HasLink<V>(T item1, T item2, V value) where V : class;
    }

    public interface ILinkedGraphUnLink<T> : ILinkedGraph<T>
    {
        bool UnLink(T item1, T item2);
    }

    public interface ILinkedGraphLink<T> : ILinkedGraph<T>
    {
        void Link<V>(T item1, T item2, V value) where V : class;
    }

    public interface ILinkedGraphTryGetLinkValue<T> : IReadOnlyLinkedGraph<T> 
    {
        bool TryGetLinkValue<V>(T item1, T item2, out V value) where V : class;
    }

    public interface ILinkedGraphNextLast<T> : IReadOnlyLinkedGraph<T> where T : class
    {
        T Next(T item);
        T Last(T item);
    }

    public interface ILinkedGraphTryNextLast<T> : IReadOnlyLinkedGraph<T> where T : class
    {
        bool TryNext(T item, out T next);
        bool TryLast(T item, out T last);
    }

    public interface ILinkedGraphValueTryGetLinkValue<T> : IReadOnlyLinkedGraph<T> 
    {
        bool TryGetLinkValue<V>(T item1, T item2, out V? value) where V : struct;
    }

    public interface ILinkedGraphValueLink<T> : ILinkedGraph<T>
    {
        void Link<V>(T item1, T item2, V value) where V : struct;
    }

    public interface IValueLinkedGraphNextLast<T> : IReadOnlyLinkedGraph<T> where T : struct
    {
        T? Next(T item);
        T? Last(T item);
    }

    public interface IValueLinkedGraphTryNextLast<T> : IReadOnlyLinkedGraph<T> where T : struct
    {
        bool TryNext(T item, out T? next);
        bool TryLast(T item, out T? last);
    }

    public interface ILinkedGraph<T> : IDataGraph<T>, IReadOnlyLinkedGraph<T> { }
    public interface IReadOnlyLinkedGraph<T> : IDataGraph<T> { }

    #endregion
}
