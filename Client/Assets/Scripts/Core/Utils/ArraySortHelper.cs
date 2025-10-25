using System;
using System.Collections.Generic;

namespace Core.Utils
{
    internal static class ArraySortHelper
    {
        #region array

    /// <summary>
    /// 如果元素存在，则返回元素对应的下标；
    /// 如果元素不存在，则返回(-(insertion point) - 1)
    /// 即： (index + 1) * -1 可得应当插入的下标。 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="fromIndex">inclusive</param>
    /// <param name="toIndex">exclusive</param>
    /// <param name="key">要查找的元素</param>
    /// <returns></returns>
    public static int BinarySearch(int[] array, int fromIndex, int toIndex,
                                   int key) {
        int low = fromIndex;
        int high = toIndex - 1;

        while (low <= high) {
            int mid = (low + high) >> 1;
            int midVal = array[mid];

            if (midVal < key)
                low = mid + 1;
            else if (midVal > key)
                high = mid - 1;
            else
                return mid; // key found
        }
        return -(low + 1); // key not found.
    }

    /// <summary>
    /// 如果comparer返回的是 mid.CompareTo(any)，适用递增数组；
    /// 如果comparer返回的是 any.CompareTo(mid)，适用递减数组
    /// </summary>
    public static int BinarySearch<T>(T[] a, int fromIndex, int toIndex,
                                      T key, Comparer<T> c) {
        int low = fromIndex;
        int high = toIndex - 1;

        while (low <= high) {
            int mid = (low + high) >> 1;
            T midVal = a[mid];
            int cmp = c.Compare(midVal, key);
            if (cmp < 0)
                low = mid + 1;
            else if (cmp > 0)
                high = mid - 1;
            else
                return mid; // key found
        }
        return -(low + 1); // key not found.
    }

    /// <summary>
    /// 如果comparer返回的是 mid.CompareTo(any)，适用递增数组；
    /// 如果comparer返回的是 any.CompareTo(mid)，适用递减数组
    /// </summary>
    public static int BinarySearch<T>(T[] a, int fromIndex, int toIndex,
                                      Func<T, int> c) {
        int low = fromIndex;
        int high = toIndex - 1;

        while (low <= high) {
            int mid = (low + high) >> 1;
            T midVal = a[mid];
            int cmp = c.Invoke(midVal);
            if (cmp < 0)
                low = mid + 1;
            else if (cmp > 0)
                high = mid - 1;
            else
                return mid; // key found
        }
        return -(low + 1); // key not found.
    }

    #endregion

    #region list

    /// <summary>
    /// 如果comparer返回的是 mid.CompareTo(any)，适用递增数组；
    /// 如果comparer返回的是 any.CompareTo(mid)，适用递减数组
    /// </summary>
    public static int BinarySearch<T>(IList<T> a, int fromIndex, int toIndex,
                                      T key, Comparer<T> c) {
        int low = fromIndex;
        int high = toIndex - 1;

        while (low <= high) {
            int mid = (low + high) >> 1;
            T midVal = a[mid];
            int cmp = c.Compare(midVal, key);
            if (cmp < 0)
                low = mid + 1;
            else if (cmp > 0)
                high = mid - 1;
            else
                return mid; // key found
        }
        return -(low + 1); // key not found.
    }

    /// <summary>
    /// 如果comparer返回的是 mid.CompareTo(any)，适用递增数组；
    /// 如果comparer返回的是 any.CompareTo(mid)，适用递减数组
    /// </summary>
    public static int BinarySearch<T>(IList<T> a, int fromIndex, int toIndex,
                                      Func<T, int> c) {
        int low = fromIndex;
        int high = toIndex - 1;

        while (low <= high) {
            int mid = (low + high) >> 1;
            T midVal = a[mid];
            int cmp = c.Invoke(midVal);
            if (cmp < 0)
                low = mid + 1;
            else if (cmp > 0)
                high = mid - 1;
            else
                return mid; // key found
        }
        return -(low + 1); // key not found.
    }

    #endregion
    }
}