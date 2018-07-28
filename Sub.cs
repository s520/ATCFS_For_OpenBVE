// Copyright 2018 S520
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met :
//
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and / or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Runtime;
using System;

namespace ATCFS {

    /// <summary>
    /// その他機能を再現するクラス
    /// </summary>
    internal class Sub : Device {

        // --- メンバ ---
        private readonly Train train_;
        private int brake_status_;  //!< ATCブレーキの状態(0: 緩解, 1: 作動)
        private int prev_brake_status_;  //!< 1フレーム前のATCブレーキの状態(0: 緩解, 1: 作動)
        private int current_negative_;  //!< 電流値[A]の負号
        private int[] current_list_;  //!< 1桁ごと表示する電流値[A]
        private int ac_voltage_;  //!< 交流電圧
        private int cv_voltage_;  //!< 制御電圧
        private int reverser_position_;  //!< レバーサー位置(0: 中立, 1: 前, 2: 後)
        private int lcd_sw_;  //!< LCD切り替えSWの状態(0: 開放, 1: 押下)
        private int lcd_status_;  //!< LCDの状態(0: 表示1, 1: 表示2)
        private int light_sw_;  //!< 手元灯SWの状態(0: 開放, 1: 押下)
        private int light_status_;  //!< 手元灯の状態(0: 消灯, 1: 点灯)
        private int[] digital_clock_;  //!< 1桁ごと表示するデジタル時計
        private int[] speedometer_;  //!< 10km/h刻みの0系/200系用速度計の針
        internal int adj_loc_ { get; private set; }  //!< 距離表示に加算する補正値[m]
        private int brake_notch_;
        private int power_notch_;
        internal double current_ { get; private set; }  //!< 電流値[A]

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="train">Trainクラスのインスタンス</param>
        internal Sub(Train train) {
            this.train_ = train;
        }

        // --- 関数 ---
        /// <summary>
        /// ATCブレーキの緩解音を再生する関数
        /// </summary>
        private void PlayAtcAirSound() {
            brake_status_ = (this.train_.Atc.atc_brake_notch_ > 0) ? 1 : 0;
            if (brake_status_ != prev_brake_status_ && this.train_.Atc.atc_brake_notch_ == 0) {
                this.train_.Sounds.AtcAirSound.Play();
            }
            prev_brake_status_ = brake_status_;
        }

        /// <summary>
        /// LCD切り替えSWが押下された際に呼び出される関数
        /// </summary>
        private void LcdSwDown() {
            if (lcd_sw_ == 0) {
                lcd_sw_ = 1;
                lcd_status_ = (lcd_status_ == 0) ? 1 : 0;
                this.train_.Sounds.LcdSwDownSound.Play();
            }
        }

        /// <summary>
        /// LCD切り替えSWが開放された際に実行する関数
        /// </summary>
        private void LcdSwUp() {
            if (lcd_sw_ == 1) {
                lcd_sw_ = 0;
                this.train_.Sounds.LcdSwUpSound.Play();
            }
        }

        /// <summary>
        /// 手元灯SWが押下された際に呼び出される関数
        /// </summary>
        private void LightSwDown() {
            if (light_sw_ == 0) {
                light_sw_ = 1;
                light_status_ = (light_status_ == 0) ? 1 : 0;
                this.train_.Sounds.LightSwDownSound.Play();
            }
        }

        /// <summary>
        /// 手元灯SWが開放された際に実行する関数
        /// </summary>
        private void LightSwUp() {
            if (light_sw_ == 1) {
                light_sw_ = 0;
                this.train_.Sounds.LightSwUpSound.Play();
            }
        }

        /// <summary>
        /// 距離表示に加算する補正値を受け取る関数
        /// </summary>
        /// <param name="distance">加算距離[m]</param>
        private void SetAdjLoc(int distance) {
            if (distance > 0) {
                adj_loc_ = distance;
            }
        }

        /// <summary>
        /// 1桁ごと分割された時刻を出力する関数
        /// </summary>
        private void DisplayClock(int time) {
            int hour = time / 3600000;
            int min = time / 60000 % 60;
            int sec = time / 1000 % 60;
            digital_clock_[0] = hour / 10;
            digital_clock_[1] = hour % 10;
            digital_clock_[2] = min / 10;
            digital_clock_[3] = min % 10;
            digital_clock_[4] = sec / 10;
            digital_clock_[5] = sec % 10;
        }

        /// <summary>
        /// 1桁ごと分割された電流値を出力する関数
        /// </summary>
        private void DisplayCurrent() {
            if (current_ < 0) {
                current_negative_ = 1;
            } else {
                current_negative_ = 0;
            }
            int current_abs = (int)(Math.Abs(current_) * 10);
            int current100 = current_abs / 1000;
            int current10 = (current_abs % 1000) / 100;
            int current1 = (current_abs % 100) / 10;
            int current_decimal1 = current_abs % 10;
            if (current100 == 10) {
                for (int i = 0; i < current_list_.Length; i++) {
                    current_list_[i] = 9;
                }
            } else {
                current_list_[0] = (current100 == 0) ? 10 : current100;
                current_list_[1] = (current100 == 0 && current10 == 0) ? 10 : current10;
                current_list_[2] = current1;
                current_list_[3] = current_decimal1;
            }
        }

        /// <summary>
        /// 0系/200系用速度計の針の表示を初期化する関数
        /// </summary>
        private void ResetSpeedometer() {
            for (int i = 0; i < speedometer_.Length; i++) { speedometer_[i] = 11; }
        }

        /// <summary>
        /// 0系/200系用速度計の針を表示する関数
        /// </summary>
        private void RunSpeedometer() {
            int speed10 = (int)Math.Abs(this.train_.State.Speed.KilometersPerHour) / 10;
            int speed1 = (int)Math.Abs(this.train_.State.Speed.KilometersPerHour) % 10;
            if (speed10 > speedometer_.Length - 1) {
                speedometer_[speedometer_.Length - 1] = 10;
            } else {
                speedometer_[speed10] = speed1;
            }
        }

        /// <summary>
        /// 現在の電流値を取得する関数
        /// </summary>
        private void GetCurrent() {
            int train_spd = (int)Math.Abs(this.train_.State.Speed.KilometersPerHour * 1000);
            if (LoadCurrent.brake_current_.Count != 0 && LoadCurrent.power_current_.Count != 0) {
                if (train_spd != 0 && brake_notch_ > 0) {
                    int brake_stage = brake_notch_;
                    if (brake_stage > LoadCurrent.brake_current_.Count) {
                        brake_stage = LoadCurrent.brake_current_.Count;
                    }
                    current_ = -BaseFunc.ListGetOrDefaultEx(LoadCurrent.brake_spd_index_, LoadCurrent.brake_current_[brake_stage - 1], train_spd);
                } else if (train_spd != 0 && power_notch_ > 0) {
                    int power_stage = power_notch_;
                    if (power_stage > LoadCurrent.power_current_.Count) {
                        power_stage = LoadCurrent.power_current_.Count;
                    }
                    current_ = BaseFunc.ListGetOrDefaultEx(LoadCurrent.power_spd_index_, LoadCurrent.power_current_[power_stage - 1], train_spd);
                } else {
                    current_ = 0.0;
                }
            }
        }

        // --- 継承された関数 ---
        /// <summary>
        /// ゲーム開始時に呼び出される関数
        /// </summary>
        /// <param name="mode">初期化モード</param>
        internal override void Initialize(InitializationModes mode) {
            ac_voltage_ = 25;
            cv_voltage_ = 100;
            digital_clock_ = new int[6];
            current_list_ = new int[4];
            speedometer_ = new int[28];
        }

        /// <summary>
        /// 1フレームごとに呼び出される関数
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="blocking">保安装置がブロックされているか、後続の保安装置をブロックするかどうか。</param>
        internal override void Elapse(ElapseData data, ref bool blocking) {
            power_notch_ = data.Handles.PowerNotch;
            brake_notch_ = data.Handles.BrakeNotch;
            PlayAtcAirSound();
            DisplayClock((int)data.TotalTime.Seconds);
            DisplayCurrent();
            ResetSpeedometer();
            RunSpeedometer();
            GetCurrent();

            // --- パネル ---
            this.train_.Panel[32] = reverser_position_;  // レバーサ表示
            this.train_.Panel[61] = lcd_status_;  // LCD表示
            this.train_.Panel[62] = light_status_;  // 手元灯
            this.train_.Panel[200] = digital_clock_[0];  // デジタル時計 (時)の十の位
            this.train_.Panel[201] = digital_clock_[1];  // デジタル時計 (時)の一の位
            this.train_.Panel[202] = digital_clock_[2];  // デジタル時計 (分)の十の位
            this.train_.Panel[203] = digital_clock_[3];  // デジタル時計 (分)の一の位
            this.train_.Panel[204] = digital_clock_[4];  // デジタル時計 (秒)の十の位
            this.train_.Panel[205] = digital_clock_[5];  // デジタル時計 (秒)の一の位
            this.train_.Panel[206] = (int)(current_ * 1000);  // 電流値[A * 1000] (力行: +, ブレーキ: -)
            this.train_.Panel[207] = (int)(Math.Abs(current_ * 1000));  // 電流値[A * 1000] (力行およびブレーキ: +)
            this.train_.Panel[208] = current_negative_;  // 電流値[A]の負号
            this.train_.Panel[209] = current_list_[0];  // 電流値[A]の百の位
            this.train_.Panel[210] = current_list_[1];  // 電流値[A]の十の位
            this.train_.Panel[211] = current_list_[2];  // 電流値[A]の一の位
            this.train_.Panel[212] = current_list_[3];  // 電流値[A]の小数第一位
            this.train_.Panel[214] = ac_voltage_;  // 電車線電圧計
            this.train_.Panel[215] = cv_voltage_;  // インバータ電圧計
            for (int i = 0; i < speedometer_.Length; i++) {
                this.train_.Panel[220 + i] = speedometer_[i];  // 0系/200系用速度計の針
            }
        }

        /// <summary>
        /// レバーサーが扱われたときに呼び出される関数
        /// </summary>
        /// <param name="reverser">レバーサ位置</param>
        internal override void SetReverser(int reverser) {
            // レバーサの位置を表示する
            switch (reverser) {
            case -1:
                reverser_position_ = 2;
                break;
            case 1:
                reverser_position_ = 1;
                break;
            default:
                reverser_position_ = 0;
                break;
            }
        }

        /// <summary>
        /// 主ハンドルが扱われたときに呼び出される関数
        /// </summary>
        /// <param name="powerNotch">力行ノッチ</param>
        internal override void SetPower(int powerNotch) {
        }

        /// <summary>
        /// ブレーキが扱われたときに呼び出される関数
        /// </summary>
        /// <param name="brakeNotch">ブレーキノッチ</param>
        internal override void SetBrake(int brakeNotch) {
        }

        /// <summary>
        /// ATSキーが押されたときに呼び出される関数
        /// </summary>
        /// <param name="key">ATSキー</param>
        internal override void KeyDown(VirtualKeys key) {
            switch (key) {
            case VirtualKeys.I:  // 手元灯
                LightSwDown();
                break;
            case VirtualKeys.L:  // LCD表示切り替え
                LcdSwDown();
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// ATSキーが離されたときに呼び出される関数
        /// </summary>
        /// <param name="key">ATSキー</param>
        internal override void KeyUp(VirtualKeys key) {
            switch (key) {
            case VirtualKeys.I:  // 手元灯
                LightSwUp();
                break;
            case VirtualKeys.L:  // LCD表示切り替え
                LcdSwUp();
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// 警笛が扱われたときに呼び出される関数
        /// </summary>
        /// <param name="type">警笛のタイプ</param>
        internal override void HornBlow(HornTypes type) {
        }

        /// <summary>
        /// 現在の閉塞の信号が変化したときに呼び出される関数
        /// </summary>
        /// <param name="signal">信号番号</param>
        internal override void SetSignal(SignalData[] signal) {
        }

        /// <summary>
        /// 地上子を越えたときに呼び出される関数
        /// </summary>
        /// <param name="beacon">車上子で受け取った情報</param>
        internal override void SetBeacon(BeaconData beacon) {
            switch (beacon.Type) {
            case 100:
                SetAdjLoc(beacon.Optional);
                break;
            default:
                break;
            }
        }
    }
}