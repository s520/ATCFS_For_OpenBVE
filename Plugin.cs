using OpenBveApi.Runtime;

namespace ATCFS {

    /// <summary>プラグインによって実装されるインタフェース</summary>
    public class Plugin : IRuntime {

        // --- メンバ ---
        private Train Train = null;
        private LoadConfig LoadConfig = null;

        // --- インターフェース関数 ---
        /// <summary>プラグインが読み込まれたときに呼び出される関数</summary>
        /// <param name="properties">読み込み時にプラグインに提供されるプロパティ</param>
        /// <returns>プラグインが正常にロードされたかどうか</returns>
        public bool Load(LoadProperties properties) {
            properties.Panel = new int[256];
            properties.AISupport = AISupport.None;
            this.Train = new Train(properties.Panel, properties.PlaySound);
            return true;
        }

        /// <summary>プラグインが解放されたときに呼び出される関数</summary>
        public void Unload() {
        }

        /// <summary>車両読み込み時に呼び出される関数</summary>
        /// <param name="specs">車両諸元</param>
        public void SetVehicleSpecs(VehicleSpecs specs) {
            this.Train.Specs = specs;
        }

        /// <summary>ゲーム開始時に呼び出される関数</summary>
        /// <param name="mode">初期化モード</param>
        public void Initialize(InitializationModes mode) {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string cfgPath = System.IO.Path.ChangeExtension(dllPath, ".cfg");
            this.LoadConfig = LoadConfig.GetInstance();
            LoadConfig.LoadCfgFile(cfgPath);
            this.Train.Initialize(mode);
        }

        /// <summary>1フレームごとに呼び出される関数</summary>
        /// <param name="data">プラグインへ渡されるデータ</param>
        public void Elapse(ElapseData data) {
            this.Train.Elapse(data);
        }

        /// <summary>レバーサーが扱われたときに呼び出される関数</summary>
        /// <param name="reverser">レバーサ位置</param>
        public void SetReverser(int reverser) {
            this.Train.SetReverser(reverser);
        }

        /// <summary>主ハンドルが扱われたときに呼び出される関数</summary>
        /// <param name="powerNotch">力行ノッチ</param>
        public void SetPower(int powerNotch) {
            this.Train.SetPower(powerNotch);
        }

        /// <summary>ブレーキが扱われたときに呼び出される関数</summary>
        /// <param name="brakeNotch">ブレーキノッチ</param>
        public void SetBrake(int brakeNotch) {
            this.Train.SetBrake(brakeNotch);
        }

        /// <summary>ATSキーが押されたときに呼び出される関数</summary>
        /// <param name="key">ATSキー</param>
        public void KeyDown(VirtualKeys key) {
            this.Train.KeyDown(key);
        }

        /// <summary>ATSキーが離されたときに呼び出される関数</summary>
        /// <param name="key">ATSキー</param>
        public void KeyUp(VirtualKeys key) {
            this.Train.KeyUp(key);
        }

        /// <summary>警笛が扱われたときに呼び出される関数</summary>
        /// <param name="type">警笛のタイプ</param>
        public void HornBlow(HornTypes type) {
            this.Train.HornBlow(type);
        }

        /// <summary>Is called when the state of the doors changes.</summary>
        /// <param name="oldState">The old state of the doors.</param>
        /// <param name="newState">The new state of the doors.</param>
        public void DoorChange(DoorStates oldState, DoorStates newState) {
            this.Train.Doors = newState;
            this.Train.DoorChange(oldState, newState);
        }

        /// <summary>現在の閉塞の信号が変化したときに呼び出される関数</summary>
        /// <param name="signal">信号番号</param>
        public void SetSignal(SignalData[] signal) {
            this.Train.SetSignal(signal);
        }

        /// <summary>地上子を越えたときに呼び出される関数</summary>
        /// <param name="beacon">車上子で受け取った情報</param>
        public void SetBeacon(BeaconData beacon) {
            this.Train.SetBeacon(beacon);
        }

        /// <summary>Is called when the plugin should perform the AI.</summary>
        /// <param name="data">The AI data.</param>
        public void PerformAI(AIData data) {
        }
    }
}