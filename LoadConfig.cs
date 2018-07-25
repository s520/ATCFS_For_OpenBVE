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

        internal static int WiperRate { get; private set; }
        internal static int WiperHoldPosition { get; private set; }
        internal static int WiperDelay { get; private set; }
        internal static int WiperSoundBehaviour { get; private set; }
        internal static int WiperWet { get; private set; }

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

            WiperRate = 700;
            WiperHoldPosition = 0;
            WiperDelay = 700;
            WiperSoundBehaviour = 0;
            WiperWet = 0;

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
                if (int.TryParse(parsedData["Wiper"]["WiperRate"], out value)) {
                    WiperRate = value;
                }
                if (int.TryParse(parsedData["Wiper"]["WiperHoldPosition"], out value)) {
                    WiperHoldPosition = value;
                }
                if (int.TryParse(parsedData["Wiper"]["WiperDelay"], out value)) {
                    WiperDelay = value;
                }
                if (int.TryParse(parsedData["Wiper"]["WiperSoundBehaviour"], out value)) {
                    WiperSoundBehaviour = value;
                }
                if (int.TryParse(parsedData["Wiper"]["WiperWet"], out value)) {
                    WiperWet = value;
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
                if (int.TryParse(parsedData["ATC"]["AtcSpeed3"], out value)) {
                    AtcSpeed[3] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed4"], out value)) {
                    AtcSpeed[4] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed5"], out value)) {
                    AtcSpeed[5] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed6"], out value)) {
                    AtcSpeed[6] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed7"], out value)) {
                    AtcSpeed[7] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed8"], out value)) {
                    AtcSpeed[8] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed9"], out value)) {
                    AtcSpeed[9] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed10"], out value)) {
                    AtcSpeed[10] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcSpeed11"], out value)) {
                    AtcSpeed[11] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcDeceleration1"], out value)) {
                    AtcDeceleration[0] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcDeceleration2"], out value)) {
                    AtcDeceleration[1] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcDeceleration3"], out value)) {
                    AtcDeceleration[2] = value;
                }
                if (int.TryParse(parsedData["ATC"]["AtcDeceleration4"], out value)) {
                    AtcDeceleration[3] = value;
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