using IniParser;
using IniParser.Model;
using System.IO;

namespace ATCFS {

    /// <summary>
    /// 設定を読み込むクラス
    /// </summary>
    internal class LoadConfig {

        // --- メンバ ---
        private static LoadConfig load_config_ = new LoadConfig();

        internal static int MaxDeceleration { get; private set; }
        internal static int LeverDelay { get; private set; }

        internal static int AtcUse { get; private set; }
        internal static int AtcType { get; private set; }
        internal static int AtcMax { get; private set; }
        internal static int[] AtcSpeed { get; private set; }
        internal static int[] AtcDeceleration { get; private set; }
        internal static int Xe { get; private set; }
        internal static int Xo { get; private set; }
        internal static int Xk { get; private set; }

        internal static int AtspUse { get; private set; }
        internal static int AtspMax { get; private set; }
        internal static int AtspDeceleration { get; private set; }

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        private LoadConfig() {
            MaxDeceleration = 2700;
            LeverDelay = 250;

            AtcUse = 1;
            AtcType = 0;
            AtcMax = 220;
            AtcSpeed = new int[12] { 0, 30, 70, 120, 170, 220, 230, 255, 270, 275, 285, 300 };
            AtcDeceleration = new int[4] { 1500, 1900, 2400, 2600 };
            Xe = 5000;
            Xo = 10000;
            Xk = 0;

            AtspUse = 0;
            AtspMax = 140;
            AtspDeceleration = 2600;
        }

        // --- 関数 ---
        /// <summary>
        /// インスタンスを取得するメソッド
        /// </summary>
        /// <returns>インスタンス</returns>
        internal static LoadConfig GetInstance() {
            return load_config_;
        }

        /// <summary>
        /// ファイルから設定を読み込む関数
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        internal void LoadCfgFile(string filePath) {
            if (File.Exists(filePath)) {
                //Create an instance of a ini file parser
                FileIniDataParser fileIniData = new FileIniDataParser();

                //Parse the ini file
                IniData parsedData = fileIniData.ReadFile(filePath);

                //Get concrete data from the ini file
                int value;
                if (int.TryParse(parsedData["Emulate"]["MaxDeceleration"], out value)) {
                    MaxDeceleration = value;
                }
                if (int.TryParse(parsedData["Emulate"]["LeverDelay"], out value)) {
                    LeverDelay = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcUse"], out value)) {
                    AtcUse = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcType"], out value)) {
                    AtcType = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcMax"], out value)) {
                    AtcMax = value;
                }
                for (int i = 3; i < AtcSpeed.Length; i++) {
                    if (int.TryParse(parsedData["ATC"]["AtcSpeed" + i], out value)) {
                        AtcSpeed[i] = value;
                    }
                }
                for (int i = 0; i < AtcDeceleration.Length; i++) {
                    if (int.TryParse(parsedData["ATC"]["AtcDeceleration" + i + 1], out value)) {
                        AtcDeceleration[0] = value;
                    }
                }
                if (int.TryParse(parsedData["ATC"]["Xe"], out value)) {
                    Xe = value;
                }
                if (int.TryParse(parsedData["ATC"]["Xo"], out value)) {
                    Xo = value;
                }
                if (int.TryParse(parsedData["ATC"]["Xk"], out value)) {
                    Xk = value;
                }
                if (int.TryParse(parsedData["ATS_P"]["AtspUse"], out value)) {
                    AtspUse = value;
                }
                if (int.TryParse(parsedData["ATS_P"]["AtspMax"], out value)) {
                    AtspMax = value;
                }
                if (int.TryParse(parsedData["ATS_P"]["AtspDeceleration"], out value)) {
                    AtspDeceleration = value;
                }
            }
        }
    }
}