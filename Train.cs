using OpenBveApi.Runtime;
using System;

namespace ATCFS {

    /// <summary>このプラグインによってシミュレートされる列車を表すクラス</summary>
    internal class Train {

        // --- クラス ---
        /// <summary>読み込みのみ可能なハンドル操作を表すクラス</summary>
        internal class ReadOnlyHandles {

            // --- メンバ ---
            /// <summary>レバーサ位置</summary>
            private int MyReverser;

            /// <summary>力行ノッチ</summary>
            private int MyPowerNotch;

            /// <summary>ブレーキノッチ</summary>
            private int MyBrakeNotch;

            /// <summary>定速制御の状態</summary>
            private bool MyConstSpeed;

            // --- プロパティ ---
            /// <summary>レバーサ位置を取得または設定する</summary>
            internal int Reverser {
                get {
                    return this.MyReverser;
                }
            }

            /// <summary>力行ノッチを取得または設定する</summary>
            internal int PowerNotch {
                get {
                    return this.MyPowerNotch;
                }
            }

            /// <summary>ブレーキノッチを取得または設定する</summary>
            internal int BrakeNotch {
                get {
                    return this.MyBrakeNotch;
                }
            }

            /// <summary>定速制御の状態を取得または設定する</summary>
            internal bool ConstSpeed {
                get {
                    return this.MyConstSpeed;
                }
            }

            // --- コンストラクタ ---
            /// <summary>新しいインスタンスを作成する</summary>
            /// <param name="handles">ハンドル操作</param>
            internal ReadOnlyHandles(Handles handles) {
                this.MyReverser = handles.Reverser;
                this.MyPowerNotch = handles.PowerNotch;
                this.MyBrakeNotch = handles.BrakeNotch;
                this.MyConstSpeed = handles.ConstSpeed;
            }
        }

        // --- プラグイン ---
        /// <summary>プラグインが現在初期化中かどうか。これは、メニューから駅にジャンプするときなどに、InitializeとElapseの間で発生する。</summary>
        internal bool PluginInitializing;

        // --- 列車 ---
        /// <summary>車両諸元</summary>
        internal VehicleSpecs Specs;

        /// <summary>現在の列車の状態</summary>
        internal VehicleState State;

        /// <summary>最後にElapseが呼び出された際のハンドル操作</summary>
        internal ReadOnlyHandles Handles;

        /// <summary>現在の客室ドアの状態</summary>
        internal DoorStates Doors;

        // --- パネルとサウンド ---
        /// <summary>パネルに渡す値</summary>
        internal int[] Panel;

        /// <summary>この列車で使用されるサウンド</summary>
        internal Sounds Sounds;

        // --- 制御装置 ---
        /// <summary>加速度</summary>
        internal Accel Accel;

        /// <summary>ATC</summary>
        internal Atc Atc;

        /// <summary>ATS-P</summary>
        internal AtsP AtsP;

        /// <summary>ワイパー</summary>
        internal Wiper Wiper;

        /// <summary>その他機能</summary>
        internal Sub Sub;

        /// <summary>この列車に搭載されているすべての保安装置のリスト。保安装置はEB、ATC、ATS-P、ATS-Sxの順でなければならない。</summary>
        internal Device[] Devices;

        // --- コンストラクタ ---
        /// <summary>新しいインスタンスを作成する</summary>
        /// <param name="panel">パネルに渡す値</param>
        /// <param name="playSound">サウンドを再生するためのデリゲート</param>
        internal Train(int[] panel, PlaySoundDelegate playSound) {
            this.PluginInitializing = false;
            this.Specs = new VehicleSpecs(0, BrakeTypes.ElectromagneticStraightAirBrake, 0, false, 0);
            this.State = new VehicleState(0.0, new Speed(0.0), 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            this.Handles = new ReadOnlyHandles(new Handles(0, 0, 0, false));
            this.Doors = DoorStates.None;
            this.Panel = panel;
            this.Sounds = new Sounds(playSound);
            this.Accel = new Accel();
            this.Atc = new Atc(this);
            this.AtsP = new AtsP(this);
            this.Wiper = new Wiper(this);
            this.Sub = new Sub(this);
            this.Devices = new Device[] { this.Accel, this.Atc, this.AtsP, this.Wiper, this.Sub };
        }

        // --- 関数 ---
        /// <summary>ゲーム開始時に呼び出される関数</summary>
        /// <param name="mode">初期化モード</param>
        internal void Initialize(InitializationModes mode) {
            this.PluginInitializing = true;
            for (int i = this.Devices.Length - 1; i >= 0; i--) {
                this.Devices[i].Initialize(mode);
            }
        }

        /// <summary>1フレームごとに呼び出される関数</summary>
        /// <param name="data">The data.</param>
        internal void Elapse(ElapseData data) {
            this.PluginInitializing = false;
            if (data.ElapsedTime.Seconds > 0.0 & data.ElapsedTime.Seconds < 1.0) {
                // --- パネル初期化 ---
                for (int i = 0; i < this.Panel.Length; i++) {
                    this.Panel[i] = 0;
                }

                // --- 保安装置 ---
                this.State = data.Vehicle;
                this.Handles = new ReadOnlyHandles(data.Handles);
                bool blocking = false;
                foreach (Device device in this.Devices) {
                    device.Elapse(data, ref blocking);
                }

                // --- パネル ---
                int seconds = (int)Math.Floor(data.TotalTime.Seconds);
                this.Panel[10] = (seconds / 3600) % 24;
                this.Panel[11] = (seconds / 60) % 60;
                this.Panel[12] = seconds % 60;
                this.Panel[194] = ((int)data.Vehicle.Location + this.Sub.adj_loc_ % 1000000) / 100000;
                this.Panel[195] = ((int)data.Vehicle.Location + this.Sub.adj_loc_ % 100000) / 10000;
                this.Panel[196] = ((int)data.Vehicle.Location + this.Sub.adj_loc_ % 10000) / 1000;
                this.Panel[197] = ((int)data.Vehicle.Location + this.Sub.adj_loc_ % 1000) / 100;
                this.Panel[198] = ((int)data.Vehicle.Location + this.Sub.adj_loc_ % 100) / 10;
                this.Panel[199] = ((int)data.Vehicle.Location + this.Sub.adj_loc_) % 10;
                this.Panel[216] = (int)(data.Vehicle.BcPressure / 98.0665 * 100.0);
                this.Panel[217] = (int)(data.Vehicle.SapPressure / 98.0665 * 100.0);
                this.Panel[218] = (int)(data.Vehicle.MrPressure / 98.0665 * 100.0);
                this.Panel[219] = (data.Vehicle.Speed.KilometersPerHour >= 30.0 && data.Handles.BrakeNotch != 0) ? 1 : 0;

                // --- サウンド ---
                this.Sounds.Elapse(data);
            }
        }

        /// <summary>レバーサーが扱われたときに呼び出される関数</summary>
        /// <param name="reverser">レバーサ位置</param>
        internal void SetReverser(int reverser) {
            foreach (Device device in this.Devices) {
                device.SetReverser(reverser);
            }
        }

        /// <summary>主ハンドルが扱われたときに呼び出される関数</summary>
        /// <param name="powerNotch">力行ノッチ</param>
        internal void SetPower(int powerNotch) {
            foreach (Device device in this.Devices) {
                device.SetPower(powerNotch);
            }
        }

        /// <summary>ブレーキが扱われたときに呼び出される関数</summary>
        /// <param name="brakeNotch">ブレーキノッチ</param>
        internal void SetBrake(int brakeNotch) {
            foreach (Device device in this.Devices) {
                device.SetBrake(brakeNotch);
            }
        }

        /// <summary>ATSキーが押されたときに呼び出される関数</summary>
        /// <param name="key">ATSキー</param>
        internal void KeyDown(VirtualKeys key) {
            foreach (Device device in this.Devices) {
                device.KeyDown(key);
            }
        }

        /// <summary>ATSキーが離されたときに呼び出される関数</summary>
        /// <param name="key">ATSキー</param>
        internal void KeyUp(VirtualKeys key) {
            foreach (Device device in this.Devices) {
                device.KeyUp(key);
            }
        }

        /// <summary>警笛が扱われたときに呼び出される関数</summary>
        /// <param name="type">警笛のタイプ</param>
        internal void HornBlow(HornTypes type) {
            foreach (Device device in this.Devices) {
                device.HornBlow(type);
            }
        }

        /// <summary>Is called when the state of the doors changes.</summary>
        /// <param name="oldState">The old state of the doors.</param>
        /// <param name="newState">The new state of the doors.</param>
        public void DoorChange(DoorStates oldState, DoorStates newState) {
        }

        /// <summary>現在の閉塞の信号が変化したときに呼び出される関数</summary>
        /// <param name="signal">信号番号</param>
        internal void SetSignal(SignalData[] signal) {
            foreach (Device device in this.Devices) {
                device.SetSignal(signal);
            }
        }

        /// <summary>地上子を越えたときに呼び出される関数</summary>
        /// <param name="beacon">車上子で受け取った情報</param>
        internal void SetBeacon(BeaconData beacon) {
            foreach (Device device in this.Devices) {
                device.SetBeacon(beacon);
            }
        }
    }
}