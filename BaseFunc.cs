using System;
using System.Collections.Generic;
using System.Linq;

namespace ATCFS {

    internal class BaseFunc {

        /// <summary>指定した値以上の先頭のインデックスを返す</summary>
        /// <typeparam name="T">比較する値の型</typeparam>
        /// <param name="arr">対象の配列（※ソート済みであること）</param>
        /// <param name="start">開始インデックス [inclusive]</param>
        /// <param name="end">終了インデックス [exclusive]</param>
        /// <param name="value">検索する値</param>
        /// <param name="comparer">比較関数(インターフェイス)</param>
        /// <returns>指定した値以上の先頭のインデックス</returns>
        internal static int LowerBound<T>(T[] arr, int start, int end, T value, IComparer<T> comparer) {
            int low = start;
            int high = end;
            int mid;
            while (low < high) {
                mid = ((high - low) >> 1) + low;
                if (comparer.Compare(arr[mid], value) < 0)
                    low = mid + 1;
                else
                    high = mid;
            }
            return low;
        }

        //引数省略のオーバーロード
        internal static int LowerBound<T>(T[] arr, T value) where T : IComparable {
            return LowerBound(arr, 0, arr.Length, value, Comparer<T>.Default);
        }

        /// <summary>指定した値より大きい先頭のインデックスを返す</summary>
        /// <typeparam name="T">比較する値の型</typeparam>
        /// <param name="arr">対象の配列（※ソート済みであること）</param>
        /// <param name="start">開始インデックス [inclusive]</param>
        /// <param name="end">終了インデックス [exclusive]</param>
        /// <param name="value">検索する値</param>
        /// <param name="comparer">比較関数(インターフェイス)</param>
        /// <returns>指定した値より大きい先頭のインデックス</returns>
        public static int UpperBound<T>(T[] arr, int start, int end, T value, IComparer<T> comparer) {
            int low = start;
            int high = end;
            int mid;
            while (low < high) {
                mid = ((high - low) >> 1) + low;
                if (comparer.Compare(arr[mid], value) <= 0)
                    low = mid + 1;
                else
                    high = mid;
            }
            return low;
        }

        //引数省略のオーバーロード
        internal static int UpperBound<T>(T[] arr, T value) {
            return UpperBound(arr, 0, arr.Length, value, Comparer<T>.Default);
        }

        /// <summary>
        /// 配列への範囲外アクセス時にデフォルト値を返す関数
        /// </summary>
        /// <typeparam name="T">配列の型</typeparam>
        /// <param name="a">配列オブジェクト</param>
        /// <param name="index">インデックス</param>
        /// <returns>配列の要素またはデフォルト値</returns>
        internal static T ArrayGetOrDefault<T>(T[] a, int index) {
            if (a.Length != 0) {
                if (index < 0) {
                    return a[0];
                } else if (index > a.Length - 1) {
                    return a.Last();
                } else {
                    return a[index];
                }
            } else {
                return (T)(object)0.0;
            }
        }

        /// <summary>
        /// 配列への範囲外アクセス時にデフォルト値を返す関数
        /// </summary>
        /// <typeparam name="T">配列の型</typeparam>
        /// <param name="l">配列オブジェクト</param>
        /// <param name="index">インデックス</param>
        /// <returns>配列の要素またはデフォルト値</returns>
        internal static T ListGetOrDefault<T>(List<T> l, int index) {
            if (l.Count() != 0) {
                if (index < 0) {
                    return l[0];
                } else if (index > l.Count() - 1) {
                    return l.Last();
                } else {
                    return l[index];
                }
            } else {
                return (T)(object)0.0;
            }
        }
    }
}