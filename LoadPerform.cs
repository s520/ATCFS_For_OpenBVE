using IniParser;
using IniParser.Model;
using System.IO;

namespace ATCFS {

    /// <summary>
    /// 車両性能ファイルを読み込むクラス
    /// </summary>
    internal class LoadPerform {

        // --- メンバ ---
        private static LoadPerform load_perform_ = new LoadPerform();
        internal static string power_current_path_ { get; private set; }
        internal static string brake_current_path_ { get; private set; }

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        private LoadPerform() {
        }

        // --- 関数 ---
        /// <summary>
        /// インスタンスを取得するメソッド
        /// </summary>
        /// <returns>インスタンス</returns>
        internal static LoadPerform GetInstance() {
            return load_perform_;
        }

        /// <summary>
        /// 車両性能ファイルを読み込む関数
        /// </summary>
        /// <param name="file_path">ファイルパス</param>
        internal void LoadCfgFile(string file_path) {
            if (File.Exists(file_path)) {
                string file_directory = System.IO.Path.GetDirectoryName(file_path);

                //Create an instance of a ini file parser
                FileIniDataParser fileIniData = new FileIniDataParser();

                //Parse the ini file
                IniData parsedData = fileIniData.ReadFile(file_path);

                //Get concrete data from the ini file
                power_current_path_ = System.IO.Path.Combine(file_directory, parsedData["Power"]["Current"]);
                brake_current_path_ = System.IO.Path.Combine(file_directory, parsedData["Brake"]["Current"]);
            }
        }
    }
}