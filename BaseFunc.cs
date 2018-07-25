using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

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

        /// <summary>指定した値以上の先頭のインデックスを返す</summary>
        /// <typeparam name="T">比較する値の型</typeparam>
        /// <param name="list">対象のリスト（※ソート済みであること）</param>
        /// <param name="start">開始インデックス [inclusive]</param>
        /// <param name="end">終了インデックス [exclusive]</param>
        /// <param name="value">検索する値</param>
        /// <param name="comparer">比較関数(インターフェイス)</param>
        /// <returns>指定した値以上の先頭のインデックス</returns>
        internal static int LowerBound<T>(List<T> list, int start, int end, T value, IComparer<T> comparer) {
            int low = start;
            int high = end;
            int mid;
            while (low < high) {
                mid = ((high - low) >> 1) + low;
                if (comparer.Compare(list[mid], value) < 0)
                    low = mid + 1;
                else
                    high = mid;
            }
            return low;
        }

        //引数省略のオーバーロード
        internal static int LowerBound<T>(List<T> list, T value) where T : IComparable {
            return LowerBound(list, 0, list.Count, value, Comparer<T>.Default);
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

        /// <summary>
        /// 線形補間を行う関数
        /// </summary>
        /// <param name="x0">x0</param>
        /// <param name="y0">y0</param>
        /// <param name="x1">x1</param>
        /// <param name="y1">y1</param>
        /// <param name="x">x</param>
        /// <returns>線形補間された値</returns>
        internal static int Lerp(int x0, int y0, int x1, int y1, int x) {
            if (x1 == x0) {
                return y0;
            } else {
                return (int)(y0 + (y1 - y0) * (x - x0) / (double)(x1 - x0));
            }
        }

        /// <summary>
        /// 線形補間を行う関数
        /// </summary>
        /// <param name="x0">x0</param>
        /// <param name="y0">y0</param>
        /// <param name="x1">x1</param>
        /// <param name="y1">y1</param>
        /// <param name="x">x</param>
        /// <returns>線形補間された値</returns>
        internal static double Lerp(double x0, double y0, double x1, double y1, double x) {
            if (x1 == x0) {
                return y0;
            } else {
                return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
            }
        }

        /// <summary>
        /// ヘッダー付きCSVファイルのデータをDataTableに読み込む関数
        /// </summary>
        /// <param name="csv_path">ヘッダー付きCSVファイルのファイルパス</param>
        /// <param name="dt">DataTableのオブジェクト</param>
        internal static void CsvToDataTable(string csv_path, DataTable dt) {
            if (File.Exists(csv_path)) {
                using (CsvReader csv = new CsvReader(new StreamReader(csv_path, Encoding.GetEncoding("Shift_JIS")), true)) {
                    int field_count = csv.FieldCount;
                    string[] headers = csv.GetFieldHeaders();
                    for (int i = 0; i < field_count; i++) {
                        dt.Columns.Add(new DataColumn(headers[i], typeof(String)));
                    }
                    while (csv.ReadNextRecord()) {
                        DataRow work_row = dt.NewRow();
                        for (int i = 0; i < field_count; i++) {
                            work_row[headers[i]] = csv[i];
                        }
                        dt.Rows.Add(work_row);
                    }
                }
            }
        }

        /// <summary>
        /// DataTableから選択列をリストへ出力する関数
        /// </summary>
        /// <param name="dt">DataTableのオブジェクト</param>
        /// <param name="header">選択列のヘッダー名</param>
        /// <param name="list">リストのオブジェクト</param>
        internal static void DataTableToList(DataTable dt, string header, ref List<int> list) {
            // Listへ一時的変換(空セル保持)
            List<string> tmp_list = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++) {
                tmp_list.Add(dt.Rows[i][header].ToString());
            }

            // Listの中間にある空文字はnullに一時的に変換
            List<int?> tmp_list2 = new List<int?>();
            for (int i = 0; i < tmp_list.Count(); i++) {
                int value = 0;
                if (!int.TryParse(tmp_list[i], out value)) {
                    if (i == 0) {
                        tmp_list2.Add(0);
                    } else if (i != tmp_list.Count() - 1) {
                        tmp_list2.Add(null);
                    }
                } else {
                    tmp_list2.Add(value);
                }
            }
            for (int i = 1; i < tmp_list2.Count(); i++) {
                if (tmp_list2[i] == null) {
                    if (i == tmp_list2.Count() - 1) {
                        tmp_list2.RemoveAt(i);
                    } else {
                        int next_full_index = i;
                        while (next_full_index < tmp_list2.Count() - 1 && tmp_list2[next_full_index] == null) {
                            next_full_index++;
                        }
                        if (tmp_list2[next_full_index] == null) {
                            tmp_list2.RemoveRange(i, next_full_index - i + 1);
                        } else {
                            tmp_list2[i] = Lerp(i - 1, (int)tmp_list2[i - 1], next_full_index, (int)tmp_list2[next_full_index], i);
                        }
                    }
                }
            }
            list = tmp_list2.Select(value => (int)value).ToList();
        }

        /// <summary>
        /// DataTableから選択列をリストへ出力する関数
        /// </summary>
        /// <param name="dt">DataTableのオブジェクト</param>
        /// <param name="header">選択列のヘッダー名</param>
        /// <param name="list">リストのオブジェクト</param>
        internal static void DataTableToList(DataTable dt, string header, ref List<double> list) {
            // Listへ一時的変換(空セル保持)
            List<string> tmp_list = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++) {
                tmp_list.Add(dt.Rows[i][header].ToString());
            }

            // Listの中間にある空文字はnullに一時的に変換
            List<double?> tmp_list2 = new List<double?>();
            for (int i = 0; i < tmp_list.Count(); i++) {
                double value = 0;
                if (!double.TryParse(tmp_list[i], out value)) {
                    if (i == 0) {
                        tmp_list2.Add(0.0);
                    } else if (i != tmp_list.Count() - 1) {
                        tmp_list2.Add(null);
                    }
                } else {
                    tmp_list2.Add(value);
                }
            }
            for (int i = 1; i < tmp_list2.Count(); i++) {
                if (tmp_list2[i] == null) {
                    if (i == tmp_list2.Count() - 1) {
                        tmp_list2.RemoveAt(i);
                    } else {
                        int next_full_index = i;
                        while (next_full_index < tmp_list2.Count() - 1 && tmp_list2[next_full_index] == null) {
                            next_full_index++;
                        }
                        if (tmp_list2[next_full_index] == null) {
                            tmp_list2.RemoveRange(i, next_full_index - i + 1);
                        } else {
                            tmp_list2[i] = Lerp(i - 1, (double)tmp_list2[i - 1], next_full_index, (double)tmp_list2[next_full_index], i);
                        }
                    }
                }
            }
            list = tmp_list2.Select(value => (double)value).ToList();
        }

        /// <summary>
        /// 検索する値に対応する値または線形補間を行った値を返す関数
        /// </summary>
        /// <param name="index_list">対象リストのインデックスを含むリストのオブジェクト</param>
        /// <param name="tget_list">対象リストのオブジェクト</param>
        /// <param name="index">検索する値</param>
        /// <returns>対応する値</returns>
        internal static double ListGetOrDefaultEx(List<int> index_list, List<double> tget_list, int index) {
            int correspond_index = index_list.IndexOf(index);
            if (correspond_index == -1) {
                int back_index = LowerBound(index_list, index);
                int front_index = back_index - 1;
                return Lerp(ListGetOrDefault(index_list, front_index), ListGetOrDefault(tget_list, front_index), ListGetOrDefault(index_list, back_index), ListGetOrDefault(tget_list, back_index), index);
            } else {
                return ListGetOrDefault(tget_list, correspond_index);
            }
        }
    }
}