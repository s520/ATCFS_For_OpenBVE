using IniParser;
using IniParser.Model;
using OpenBveApi.Runtime;
using System.IO;

namespace ATCFS {

    /// <summary>
    /// カスタムスイッチの設定を読み込むクラス
    /// </summary>
    internal class LoadSwitch {

        // --- メンバ ---
        private static LoadSwitch load_switch_ = new LoadSwitch();
        internal const int ALL_SWITCH = 10;

        /// <summary>
        /// カスタムスイッチの設定を格納するクラス
        /// </summary>
        internal class SwitchConfig {
            internal int switch_index_ { get; private set; }
            internal int switch_min_ { get; private set; }
            internal int switch_max_ { get; private set; }
            internal int switch_init_ { get; private set; }
            internal int switch_step_ { get; private set; }
            internal int switch_is_loop_ { get; private set; }
            internal VirtualKeys?[] switch_key_ { get; private set; }

            // --- コンストラクタ ---
            /// <summary>
            /// 新しいインスタンスを作成する
            /// </summary>
            internal SwitchConfig() {
                switch_index_ = 271;
                switch_min_ = 0;
                switch_max_ = 1;
                switch_init_ = 0;
                switch_step_ = 1;
                switch_is_loop_ = 0;
                switch_key_ = new VirtualKeys?[2] { null, null };
            }

            // --- 関数 ---
            /// <summary>
            /// キー割り当てを読み込むクラス
            /// </summary>
            /// <param name="key">string型のキー割り当て</param>
            /// <returns>VirtualKeys型のキー割り当て</returns>
            private VirtualKeys? LoadKeyCfg(string key) {
                if (key == "S") {
                    return VirtualKeys.S;
                } else if (key == "A1") {
                    return VirtualKeys.A1;
                } else if (key == "A2") {
                    return VirtualKeys.A2;
                } else if (key == "B1") {
                    return VirtualKeys.B1;
                } else if (key == "B2") {
                    return VirtualKeys.B2;
                } else if (key == "C1") {
                    return VirtualKeys.C1;
                } else if (key == "C2") {
                    return VirtualKeys.C2;
                } else if (key == "D") {
                    return VirtualKeys.D;
                } else if (key == "E") {
                    return VirtualKeys.E;
                } else if (key == "F") {
                    return VirtualKeys.F;
                } else if (key == "G") {
                    return VirtualKeys.G;
                } else if (key == "H") {
                    return VirtualKeys.H;
                } else if (key == "I") {
                    return VirtualKeys.I;
                } else if (key == "J") {
                    return VirtualKeys.J;
                } else if (key == "K") {
                    return VirtualKeys.K;
                } else if (key == "L") {
                    return VirtualKeys.L;
                } else {
                    return null;
                }
            }

            /// <summary>
            /// ファイルから設定を読み込む関数
            /// </summary>
            /// <param name="file_path">ファイルパス</param>
            /// <param name="i">カスタムスイッチのナンバー</param>
            internal void LoadCfgFile(string file_path, int i) {
                if (File.Exists(file_path)) {
                    //Create an instance of a ini file parser
                    FileIniDataParser file_ini_data = new FileIniDataParser();

                    //Parse the ini file
                    IniData parsed_data = file_ini_data.ReadFile(file_path);

                    //Get concrete data from the ini file
                    int value;
                    if (int.TryParse(parsed_data["Switch" + i]["Index"], out value)) {
                        switch_index_ = value;
                    }
                    if (int.TryParse(parsed_data["Switch" + i]["Min"], out value)) {
                        switch_min_ = value;
                    }
                    if (int.TryParse(parsed_data["Switch" + i]["Max"], out value)) {
                        switch_max_ = value;
                    }
                    if (int.TryParse(parsed_data["Switch" + i]["Init"], out value)) {
                        switch_init_ = value;
                    }
                    if (int.TryParse(parsed_data["Switch" + i]["Step"], out value)) {
                        switch_step_ = value;
                    }
                    if (int.TryParse(parsed_data["Switch" + i]["Loop"], out value)) {
                        switch_is_loop_ = value;
                    }
                    for (int j = 0; j < 2; j++) {
                        switch_key_[j] = LoadKeyCfg(parsed_data["Switch" + i]["Key" + j]);
                    }
                }
            }
        }

        internal static SwitchConfig[] switch_config_;

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        private LoadSwitch() {
            switch_config_ = new SwitchConfig[ALL_SWITCH];
            for (int i = 0; i < ALL_SWITCH; i++) {
                switch_config_[i] = new SwitchConfig();
            }
        }

        // --- 関数 ---
        /// <summary>
        /// インスタンスを取得するメソッド
        /// </summary>
        /// <returns>インスタンス</returns>
        internal static LoadSwitch GetInstance() {
            return load_switch_;
        }

        /// <summary>
        /// ファイルから設定を読み込む関数
        /// </summary>
        /// <param name="file_path">ファイルパス</param>
        internal void LoadCfgFile(string file_path) {
            for (int i = 0; i < ALL_SWITCH; i++) {
                switch_config_[i].LoadCfgFile(file_path, i);
            }
        }
    }
}