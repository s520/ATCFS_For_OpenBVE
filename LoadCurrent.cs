using System.Collections.Generic;
using System.Data;

namespace ATCFS {

    /// <summary>
    /// 電流テーブルを読み込むクラス
    /// </summary>
    internal class LoadCurrent {

        // --- メンバ ---
        private static LoadCurrent load_current_ = new LoadCurrent();
        internal static List<int> power_spd_index_ { get; private set; }
        internal static List<List<double>> power_current_ { get; private set; }
        internal static List<int> brake_spd_index_ { get; private set; }
        internal static List<List<double>> brake_current_ { get; private set; }

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        private LoadCurrent() {
            power_spd_index_ = new List<int>();
            power_current_ = new List<List<double>>();
            brake_spd_index_ = new List<int>();
            brake_current_ = new List<List<double>>();
        }

        // --- 関数 ---
        /// <summary>
        /// インスタンスを取得するメソッド
        /// </summary>
        /// <returns>インスタンス</returns>
        internal static LoadCurrent GetInstance() {
            return load_current_;
        }

        /// <summary>
        /// 力行ノッチの電流テーブルを読み込む関数
        /// </summary>
        /// <param name="csv_path">CSVファイルパス</param>
        internal void LoadPowerCfg(string csv_path) {
            DataTable current_dt = new DataTable();
            BaseFunc.CsvToDataTable(csv_path, current_dt);
            List<int> work_list = new List<int>();
            BaseFunc.DataTableToList(current_dt, "TrainSpeed", ref work_list);
            for (int i = 0; i < work_list.Count; i++) {
                power_spd_index_.Add(work_list[i] * 1000);
            }
            for (int i = 1; i < current_dt.Columns.Count; i++) {
                List<double> work_list2 = new List<double>();
                BaseFunc.DataTableToList(current_dt, "Stage" + i, ref work_list2);
                power_current_.Add(work_list2);
            }
        }

        /// <summary>
        /// ブレーキノッチの電流テーブルを読み込む関数
        /// </summary>
        /// <param name="csv_path">CSVファイルパス</param>
        internal void LoadBrakeCfg(string csv_path) {
            DataTable current_dt = new DataTable();
            BaseFunc.CsvToDataTable(csv_path, current_dt);
            List<int> work_list = new List<int>();
            BaseFunc.DataTableToList(current_dt, "TrainSpeed", ref work_list);
            for (int i = 0; i < work_list.Count; i++) {
                brake_spd_index_.Add(work_list[i] * 1000);
            }
            for (int i = 1; i < current_dt.Columns.Count; i++) {
                List<double> work_list2 = new List<double>();
                BaseFunc.DataTableToList(current_dt, "Stage" + i, ref work_list2);
                brake_current_.Add(work_list2);
            }
        }
    }
}